using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Solution.SolutionMapper;

public class SolutionMapper
    {
        private readonly List<object> _rules = new();
        private readonly Dictionary<(Type, Type), object> _typeConverters = new();

        public void AddProfile(SolutionMapperProfile profile)
        {
            _rules.AddRange(profile.Rules);
        }

        public void AddMapping<TSource, TDest>(SolutionMappingExpression<TSource, TDest> mapping)
        {
            _rules.Add(mapping);
        }

        public void AddTypeConverter<TSource, TDest>(ITypeConverter<TSource, TDest> converter)
        {
            _typeConverters[(typeof(TSource), typeof(TDest))] = converter;
        }

        public TDest Map<TSource, TDest>(TSource source) where TDest : new()
        {
            return Map<TSource, TDest>(source, new ResolutionContext());
        }

        public TDest Map<TSource, TDest>(TSource source, ResolutionContext context)
            where TDest : new()
        {
            var rule = _rules.OfType<SolutionMappingExpression<TSource, TDest>>().FirstOrDefault();
            if (rule == null)
            {
                throw new InvalidOperationException(
                    $"Mapping rule not found for {typeof(TSource).Name} → {typeof(TDest).Name}. " +
                    $"Configura CreateMap<{typeof(TSource).Name}, {typeof(TDest).Name}> o usa ReverseMap nel profilo."
                );
            }
            TDest dest;

            if (rule?.CustomCtor != null)
                dest = rule.CustomCtor(source);
            else if (rule?.CustomCtorWithContext != null)
                dest = rule.CustomCtorWithContext(source, context);
            else
                dest = new TDest();

            rule?.BeforeMapAction?.Invoke(source, dest);
            rule?.BeforeMapActionWithContext?.Invoke(source, dest, context);

            if (rule?.TypeConverter != null)
                return rule.TypeConverter.Convert(source, dest, context);
            if (rule?.CustomMappingFunction != null)
                return rule.CustomMappingFunction(source, dest, context);
            if (rule?.CustomMappingExpression != null)
                return rule.CustomMappingExpression.Compile().Invoke(source);

            if (rule != null && rule.Members.Count > 0)
            {
                // Mappa le proprietà definite con ForMember
                foreach (var (destProp, getter, valueConverter) in rule.Members)
                {
                    if (rule.IgnoredMembers.Contains(destProp.Name)) continue;
                    var value = getter(source);

                    if (valueConverter != null)
                    {
                        var method = valueConverter.GetType().GetMethod("Convert");
                        value = method.Invoke(valueConverter, new[] { value, destProp.GetValue(dest), context });
                    }
                    else
                    {
                        var srcType = value?.GetType();
                        var destType = destProp.PropertyType;
                        if (srcType != null && IsComplexType(srcType) && IsComplexType(destType) &&
                            HasMappingRule(srcType, destType))
                        {
                            value = MapDynamic(value, destType);
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
                    if (rule.IgnoredMembers.Contains(destProp.Name) ||
                        (context?.Items.TryGetValue("IgnoreProps", out var ignoreObj) == true &&
                         ignoreObj is HashSet<string> ignoreSet &&
                         ignoreSet.Contains(destProp.Name))) continue;

                    var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name);
                    if (srcProp != null && destProp.CanWrite)
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

                        // Mapping ricorsivo per tipi complessi
                        var srcType = value?.GetType();
                        var destType = destProp.PropertyType;
                        if (value != null && IsComplexType(srcType) && IsComplexType(destType) &&
                            HasMappingRule(srcType, destType))
                        {
                            value = MapDynamic(value, destType);
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
                // Mappatura automatica classica
                var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var destProp in destProps)
                {
                    if (rule != null && rule.IgnoredMembers.Contains(destProp.Name)) continue;
                    var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name);
                    if (srcProp != null && destProp.CanWrite)
                    {
                        var value = srcProp.GetValue(source);

                        // Se il tipo è complesso e c'è una regola di mapping, usa Map ricorsivo
                        var srcType = value?.GetType();
                        var destType = destProp.PropertyType;
                        if (value != null && IsComplexType(srcType) && IsComplexType(destType) &&
                            HasMappingRule(srcType, destType))
                        {
                            value = MapDynamic(value, destType);
                        }
                        else
                        {
                            value = ConvertWithTypeConverter(value, destProp.GetValue(dest), destType, context);
                        }

                        destProp.SetValue(dest, value);
                    }
                }
            }

            rule?.AfterMapAction?.Invoke(source, dest);
            rule?.AfterMapActionWithContext?.Invoke(source, dest, context);

            return dest;
        }

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

        private bool IsComplexType(Type type)
        {
            return type.IsClass && type != typeof(string);
        }

        private bool HasMappingRule(Type srcType, Type destType)
        {
            return _rules.Any(r =>
                r.GetType().IsGenericType &&
                r.GetType().GetGenericArguments()[0] == srcType &&
                r.GetType().GetGenericArguments()[1] == destType);
        }

        public object MapDynamic(object source, Type destType)
        {
            var method = typeof(SolutionMapper)
                .GetMethods()
                .First(m => m.Name == "Map"
                            && m.IsGenericMethodDefinition
                            && m.GetParameters().Length == 1);

            var genericMethod = method.MakeGenericMethod(source.GetType(), destType);
            return genericMethod.Invoke(this, new[] { source });
        }

    }
