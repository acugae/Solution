using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Solution.SolutionMapper;

/// <summary>
/// Gestisce la configurazione e l'esecuzione delle regole di mapping tra tipi.
/// Permette di aggiungere profili, regole di mapping e converter personalizzati.
/// </summary>
public class SolutionMapper
{
	// Lista di regole di mapping (profili e mapping espliciti)
	private readonly List<object> _rules = new();
	private readonly object _rulesLock = new();
	private readonly System.Collections.Concurrent.ConcurrentDictionary<object, Delegate> _compiledExpressions = new();

	// Dizionario di converter tra tipi specifici
	private readonly Dictionary<(Type, Type), object> _typeConverters = new();

	/// <summary>
	/// Aggiunge un profilo di mapping, includendo tutte le regole definite nel profilo.
	/// </summary>
	public void AddProfile(SolutionMapperProfile profile)
	{
		lock (_rulesLock)
		{
			_rules.AddRange(profile.Rules);
		}
	}

	/// <summary>
	/// Aggiunge una regola di mapping esplicita tra TSource e TDest.
	/// </summary>
	public void AddMapping<TSource, TDest>(SolutionMappingExpression<TSource, TDest> mapping)
	{
		lock (_rulesLock)
		{
			_rules.Add(mapping);
		}
	}

	/// <summary>
	/// Aggiunge un converter personalizzato tra TSource e TDest.
	/// </summary>
	public void AddTypeConverter<TSource, TDest>(ITypeConverter<TSource, TDest> converter)
	{
		_typeConverters[(typeof(TSource), typeof(TDest))] = converter;
	}

	/// <summary>
	/// Esegue il mapping da TSource a TDest, creando un nuovo contesto di risoluzione.
	/// </summary>
	public TDest Map<TSource, TDest>(TSource source) where TDest : new()
	{
		return Map<TSource, TDest>(source, new ResolutionContext());
	}

	/// <summary>
	/// Esegue il mapping da TSource a TDest usando il contesto specificato.
	/// </summary>
	public TDest Map<TSource, TDest>(TSource source, ResolutionContext context)
		where TDest : new()
	{
		if (source == null)
			return default!;

		context ??= new ResolutionContext();

		// Protezione cicli e profondità massima (previene StackOverflowException)
		if (context.InstanceCache.TryGetValue(source, out var cached) && cached is TDest cachedDest)
			return cachedDest;

		// Check if we're mapping collections - integrato all'inizio per gestire liste direttamente
		if (IsListType(typeof(TSource)) && IsListType(typeof(TDest)))
		{
			return MapCollection<TSource, TDest>(source, context);
		}

		// Cerca la regola di mapping appropriata (thread-safe)
		SolutionMappingExpression<TSource, TDest>? rule;
		lock (_rulesLock)
		{
			rule = _rules.OfType<SolutionMappingExpression<TSource, TDest>>().FirstOrDefault();
		}
		if (rule == null)
		{
			throw new InvalidOperationException(
				$"Mapping rule not found for {typeof(TSource).Name} → {typeof(TDest).Name}. " +
				$"Configura CreateMap<{typeof(TSource).Name}, {typeof(TDest).Name}> o usa ReverseMap nel profilo."
			);
		}

		int effectiveMaxDepth = rule.MaxDepthValue ?? context.MaxDepth;
		if (effectiveMaxDepth > 0 && context.CurrentDepth >= effectiveMaxDepth)
			return default!;

		TDest dest;

		// Costruzione dell'oggetto di destinazione tramite costruttori personalizzati se presenti
		if (rule.CustomCtor != null)
			dest = rule.CustomCtor(source);
		else if (rule.CustomCtorWithContext != null)
			dest = rule.CustomCtorWithContext(source, context);
		else
			dest = new TDest();

		// Registra subito l'istanza nella cache per gestire riferimenti ciclici
		if (!typeof(TSource).IsValueType)
			context.InstanceCache[source] = dest!;

		context.CurrentDepth++;
		try
		{
			// Azioni da eseguire prima del mapping
			rule.BeforeMapAction?.Invoke(source, dest);
			rule.BeforeMapActionWithContext?.Invoke(source, dest, context);

			// Se è definito un converter o una funzione di mapping personalizzata, la si usa
			if (rule.TypeConverter != null)
				return rule.TypeConverter.Convert(source, dest, context);
			if (rule.CustomMappingFunction != null)
				return rule.CustomMappingFunction(source, dest, context);
			if (rule.CustomMappingExpression != null)
			{
				var compiled = (Func<TSource, TDest>)_compiledExpressions.GetOrAdd(rule, _ => rule.CustomMappingExpression.Compile());
				return compiled(source);
			}

			bool IsIgnored(string propName) =>
				rule.IgnoredMembers.Contains(propName) ||
				(context.Items.TryGetValue("IgnoreProps", out var ignoreObj) &&
				 ignoreObj is HashSet<string> ignoreSet &&
				 ignoreSet.Contains(propName));

			bool IsSetterAllowed(PropertyInfo prop) =>
				prop.CanWrite && (!rule.IgnoreInaccessibleSetter || (prop.SetMethod != null && prop.SetMethod.IsPublic));

			// Se sono definiti membri tramite ForMember, si mappano esplicitamente
			if (rule.Members.Count > 0)
			{
				// Mappa le proprietà definite con ForMember
				foreach (var (destProp, getter, valueConverter) in rule.Members)
				{
					if (IsIgnored(destProp.Name) || !IsSetterAllowed(destProp)) continue;
					var value = getter(source);

					if (valueConverter != null)
					{
						// Usa il converter specifico per il membro
						var method = valueConverter.GetType().GetMethod("Convert");
						value = method.Invoke(valueConverter, new[] { value, destProp.GetValue(dest), context });
					}
					else
					{
						// Gestione di tipi complessi e conversioni generiche, inclusi liste
						var srcType = value?.GetType();
						var destType = destProp.PropertyType;
						if (srcType != null && (IsComplexType(srcType) || IsListType(srcType)) &&
							(IsComplexType(destType) || IsListType(destType)) &&
							HasMappingRule(srcType, destType))
						{
							value = MapDynamic(value, destType, context);
						}
						else
						{
							value = ConvertWithTypeConverter(value, destProp.GetValue(dest), destType, context);
						}
					}

					destProp.SetValue(dest, value);
				}

				// Mappa automaticamente le proprietà non gestite da ForMember e non ignorate
				var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

				// Prendi i nomi delle proprietà già mappate
				var mappedNames = rule.Members.Select(m => m.destProp.Name).ToHashSet();

				foreach (var destProp in destProps)
				{
					if (mappedNames.Contains(destProp.Name)) continue;
					if (IsIgnored(destProp.Name)) continue;

					var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name);
					if (srcProp != null && IsSetterAllowed(destProp))
					{
						object value = null;
						try
						{
							if (srcProp.GetMethod == null || !srcProp.GetMethod.IsPublic)
								throw new InvalidOperationException($"La proprietà '{srcProp.Name}' di '{srcProp.DeclaringType?.Name}' non ha un getter pubblico.");

							value = srcProp.GetValue(source);
						}
						catch (Exception ex)
						{
							throw new InvalidOperationException(
								$"Errore nel mapping: impossibile ottenere il valore per la proprietà '{srcProp.Name}' " +
								$"({srcProp.DeclaringType?.Name}). " +
								$"Verifica che la proprietà abbia un getter pubblico e che la configurazione del mapping sia corretta. " +
								$"Dettaglio: {ex.Message}", ex);
						}

						// Mapping ricorsivo per tipi complessi o liste
						var srcType = value?.GetType();
						var destType = destProp.PropertyType;
						if (value != null && (IsComplexType(srcType) || IsListType(srcType)) &&
							(IsComplexType(destType) || IsListType(destType)) &&
							HasMappingRule(srcType, destType))
						{
							value = MapDynamic(value, destType, context);
						}
						else
						{
							value = ConvertWithTypeConverter(value, destProp.GetValue(dest), destType, context);
						}

						destProp.SetValue(dest, value);
					}
				}
			}
			else
			{
				// Mappatura automatica classica: proprietà con lo stesso nome e tipo
				var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				foreach (var destProp in destProps)
				{
					if (IsIgnored(destProp.Name)) continue;
					var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name);
					if (srcProp != null && IsSetterAllowed(destProp) && srcProp.CanRead)
					{
						var value = srcProp.GetValue(source);

						// Se il tipo è complesso o lista e c'è una regola di mapping, usa Map ricorsivo
						var srcType = value?.GetType();
						var destType = destProp.PropertyType;
						if (value != null && (IsComplexType(srcType) || IsListType(srcType)) &&
							(IsComplexType(destType) || IsListType(destType)) &&
							HasMappingRule(srcType, destType))
						{
							// Se la destinazione è una collezione interfaccia, crea una List<T> e assegna quella
							if (IsListType(destType) && destType.IsInterface)
							{
								var destElementType = GetElementType(destType);
								var listType = typeof(List<>).MakeGenericType(destElementType);
								value = MapDynamic(value, listType, context);
							}
							else
							{
								value = MapDynamic(value, destType, context);
							}
						}
						else
						{
							value = ConvertWithTypeConverter(value, destProp.GetValue(dest), destType, context);
						}

						destProp.SetValue(dest, value);
					}
				}
			}

			// Azioni da eseguire dopo il mapping
			rule.AfterMapAction?.Invoke(source, dest);
			rule.AfterMapActionWithContext?.Invoke(source, dest, context);

			return dest;
		}
		finally
		{
			context.CurrentDepth--;
		}
	}

	/// <summary>
	/// Maps collections (List, IList, ICollection, IEnumerable) element by element.
	/// </summary>
	private TDest MapCollection<TSource, TDest>(TSource sourceCollection, ResolutionContext context)
		where TDest : new()
	{
		if (sourceCollection == null)
			return default(TDest);

		var sourceType = typeof(TSource);
		var destType = typeof(TDest);

		// Get element types
		var sourceElementType = GetElementType(sourceType);
		var destElementType = GetElementType(destType);

		if (sourceElementType == null || destElementType == null)
			throw new InvalidOperationException($"Cannot determine element types for {sourceType.Name} → {destType.Name}");

		// Check if we have a mapping rule for the element types
		if (!HasMappingRule(sourceElementType, destElementType))
			throw new InvalidOperationException($"No mapping rule found for element types {sourceElementType.Name} → {destElementType.Name}");

		// Convert source to IEnumerable
		var sourceEnumerable = sourceCollection as System.Collections.IEnumerable;
		if (sourceEnumerable == null)
			throw new InvalidOperationException($"Source collection {sourceType.Name} is not IEnumerable");

		// Create destination list
		var destListType = typeof(List<>).MakeGenericType(destElementType);
		var destList = Activator.CreateInstance(destListType) as System.Collections.IList;

		// Map each element
		foreach (var sourceItem in sourceEnumerable)
		{
			if (sourceItem == null)
			{
				destList.Add(null);
				continue;
			}

			var destItem = MapDynamic(sourceItem, destElementType, context);
			destList.Add(destItem);
		}

		// Se il tipo destinazione è una delle principali interfacce collezione, restituisci direttamente la List<T>
		if (destType.IsGenericType)
		{
			var destGenDef = destType.GetGenericTypeDefinition();
			if (destGenDef == typeof(IEnumerable<>) || destGenDef == typeof(IList<>) || destGenDef == typeof(ICollection<>))
			{
				return (TDest)destList;
			}
		}

		// Se il tipo destinazione è List<T> o compatibile, restituisci la lista
		if (destType.IsGenericType && destType.GetGenericTypeDefinition() == typeof(List<>))
		{
			return (TDest)destList;
		}

		if (destType.IsAssignableFrom(destListType))
		{
			return (TDest)destList;
		}

		// Prova a creare un'istanza di TDest e popolarla
		var destInstance = new TDest();
		if (destInstance is System.Collections.IList destInstanceList)
		{
			foreach (var item in destList)
			{
				destInstanceList.Add(item);
			}
			return destInstance;
		}

		throw new InvalidOperationException($"Cannot create or populate destination collection of type {destType.Name}");
	}

	/// <summary>
	/// Checks if a type is a List or other collection type.
	/// </summary>
	private bool IsListType(Type type)
	{
		if (type == null) return false;

		return type.IsGenericType &&
			   (type.GetGenericTypeDefinition() == typeof(List<>) ||
				type.GetGenericTypeDefinition() == typeof(IList<>) ||
				type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
				type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
	}

	/// <summary>
	/// Gets the element type of a collection type.
	/// </summary>
	private Type GetElementType(Type collectionType)
	{
		if (collectionType.IsGenericType)
		{
			var genericArgs = collectionType.GetGenericArguments();
			if (genericArgs.Length == 1)
				return genericArgs[0];
		}
		return null;
	}

	/// <summary>
	/// Prova a convertire il valore usando un converter registrato, oppure usa la conversione di tipo standard.
	/// </summary>
	private object ConvertWithTypeConverter(object value, object destination, Type destType, ResolutionContext context)
	{
		if (value == null) return null;
		var srcType = value.GetType();
		if (srcType == destType || destType.IsAssignableFrom(srcType))
			return value;

		if (_typeConverters.TryGetValue((srcType, destType), out var converterObj))
		{
			var method = converterObj.GetType().GetMethod("Convert");
			return method.Invoke(converterObj, new[] { value, destination, context });
		}
		try
		{
			return Convert.ChangeType(value, destType);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Determina se il tipo è una classe complessa (escludendo le stringhe).
	/// </summary>
	private bool IsComplexType(Type type)
	{
		return type.IsClass && type != typeof(string);
	}

	/// <summary>
	/// Verifica se esiste una regola di mapping tra i due tipi, includendo il supporto per le collezioni generiche.
	/// </summary>
	private bool HasMappingRule(Type srcType, Type destType)
	{
		// Check direct mapping rule (thread-safe)
		bool directRule;
		lock (_rulesLock)
		{
			directRule = _rules.Any(r =>
				r.GetType().IsGenericType &&
				r.GetType().GetGenericArguments()[0] == srcType &&
				r.GetType().GetGenericArguments()[1] == destType);
		}

		if (directRule) return true;

		// Check collection mapping rule - per le liste, controlla se esistono regole per gli elementi
		if (IsListType(srcType) && IsListType(destType))
		{
			var srcElementType = GetElementType(srcType);
			var destElementType = GetElementType(destType);

			if (srcElementType != null && destElementType != null)
			{
				return HasMappingRule(srcElementType, destElementType);
			}
		}

		return false;
	}

	/// <summary>
	/// Esegue il mapping dinamico tra oggetti di tipo sconosciuto a tempo di compilazione.
	/// </summary>
	public object MapDynamic(object source, Type destType)
	{
		return MapDynamic(source, destType, new ResolutionContext());
	}

	private object MapDynamic(object source, Type destType, ResolutionContext context)
	{
		var srcType = source.GetType();
		// Se non esiste una regola di mapping per srcType -> destType, la crea dinamicamente (thread-safe)
		if (!HasMappingRule(srcType, destType))
		{
			// Crea una regola di mapping T -> T se i tipi coincidono
			if (srcType == destType)
			{
				lock (_rulesLock)
				{
					if (!_rules.Any(r => r.GetType().IsGenericType && r.GetType().GetGenericArguments()[0] == srcType && r.GetType().GetGenericArguments()[1] == destType))
					{
						var mappingType = typeof(SolutionMappingExpression<,>).MakeGenericType(srcType, destType);
						var mapping = Activator.CreateInstance(mappingType);
						if (mapping != null)
							_rules.Add(mapping);
					}
				}
			}
		}
		var method = typeof(SolutionMapper)
			.GetMethods()
			.First(m => m.Name == "Map"
						&& m.IsGenericMethodDefinition
						&& m.GetParameters().Length == 2);

		var genericMethod = method.MakeGenericMethod(srcType, destType);
		return genericMethod.Invoke(this, new[] { source, context });
	}
}
