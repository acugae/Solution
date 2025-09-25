using System.Linq.Expressions;
using System.Reflection;

namespace Solution.SolutionMapper;

/// <summary>
/// Configura le regole di mapping tra un tipo sorgente TSource e un tipo destinazione TDest.
/// Permette di personalizzare la mappatura di proprietà, costruttori, azioni prima/dopo il mapping, converter e altro.
/// </summary>
public class SolutionMappingExpression<TSource, TDest> : ISolutionMappingExpression<TSource, TDest>
{
    // Membri mappati esplicitamente: destinazione, funzione per ottenere il valore dal source, eventuale converter
    internal List<(PropertyInfo destProp, Func<TSource, object> getter, object valueConverter)> Members { get; } = new();

    // Nomi delle proprietà da ignorare durante il mapping
    internal HashSet<string> IgnoredMembers { get; } = new();

    // Configurazioni specifiche per membri sorgente
    internal List<(string, Action<ISourceMemberConfigurationExpression>)> SourceMemberConfigs { get; } = new();

    // Configurazioni specifiche per parametri del costruttore
    internal List<(string, Action<ICtorParamConfigurationExpression<TSource>>)> CtorParamConfigs { get; } = new();

    // Azioni da eseguire prima del mapping
    internal Action<TSource, TDest> BeforeMapAction { get; private set; }
    internal Action<TSource, TDest, ResolutionContext> BeforeMapActionWithContext { get; private set; }

    // Azioni da eseguire dopo il mapping
    internal Action<TSource, TDest> AfterMapAction { get; private set; }
    internal Action<TSource, TDest, ResolutionContext> AfterMapActionWithContext { get; private set; }

    // Converter personalizzato per la mappatura
    internal ITypeConverter<TSource, TDest> TypeConverter { get; private set; }

    // Funzione di mapping personalizzata
    internal Func<TSource, TDest, ResolutionContext, TDest> CustomMappingFunction { get; private set; }

    // Espressione di mapping personalizzata
    internal Expression<Func<TSource, TDest>> CustomMappingExpression { get; private set; }

    // Costruttore personalizzato
    internal Func<TSource, TDest> CustomCtor { get; private set; }
    internal Func<TSource, ResolutionContext, TDest> CustomCtorWithContext { get; private set; }

    // Profondità massima per il mapping ricorsivo
    internal int? MaxDepthValue { get; private set; }

    // Se true, preserva i riferimenti degli oggetti mappati
    internal bool PreserveReferencesValue { get; private set; }

    // Se true, disabilita la validazione del costruttore
    internal bool DisableCtorValidationValue { get; private set; }

    // Imposta la modalità di validazione dei membri
    internal MemberList? MemberListValidation { get; private set; }

    // Se true, include tutti i tipi derivati nel mapping
    internal bool IncludeAllDerivedValue { get; private set; }

    // Tipi derivati da includere nel mapping
    internal List<(Type, Type)> Includes { get; } = new();

    // Tipi base da includere nel mapping
    internal List<(Type, Type)> IncludeBases { get; } = new();

    // Se true, ignora le proprietà con setter non accessibili
    internal bool IgnoreInaccessibleSetter { get; private set; }

    // Se true, ignora le proprietà sorgente con setter non accessibili
    internal bool IgnoreSourceInaccessibleSetter { get; private set; }

    // Configurazioni per trasformatori di valore
    public IList<ValueTransformerConfiguration> ValueTransformers { get; } = new List<ValueTransformerConfiguration>();

    /// <summary>
    /// Configura il mapping tra una proprietà di destinazione e una di sorgente, con eventuale converter.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ForMember<TMember>(
        Expression<Func<TDest, TMember>> destSelector,
        Expression<Func<TSource, TMember>> srcSelector,
        IValueConverter<TMember, TMember> converter = null)
    {
        var destProp = (PropertyInfo)((MemberExpression)destSelector.Body).Member;
        var getter = srcSelector.Compile();
        Members.Add((destProp, src => getter(src), converter));
        return this;
    }

    /// <summary>
    /// Configura opzioni per un membro sorgente specifico.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        SourceMemberConfigs.Add((sourceMemberName, memberOptions));
        return this;
    }

    /// <summary>
    /// Ignora una proprietà di destinazione specificata per nome.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> Ignore(string field)
    {
        IgnoredMembers.Add(field);
        return this;
    }

    /// <summary>
    /// Ignora una proprietà di destinazione specificata tramite espressione.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> Ignore(Expression<Func<TDest, object>> destSelector)
    {
        var destProp = (PropertyInfo)((MemberExpression)(destSelector.Body is UnaryExpression ue ? ue.Operand : destSelector.Body)).Member;
        IgnoredMembers.Add(destProp.Name);
        return this;
    }

    /// <summary>
    /// Imposta un'azione da eseguire prima del mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest> beforeAction)
    {
        BeforeMapAction = beforeAction;
        return this;
    }

    /// <summary>
    /// Imposta un'azione da eseguire prima del mapping, con contesto.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest, ResolutionContext> beforeAction)
    {
        BeforeMapActionWithContext = beforeAction;
        return this;
    }

    /// <summary>
    /// Imposta un'azione di mapping personalizzata da eseguire prima del mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new()
    {
        BeforeMapActionWithContext = (src, dest, ctx) => new TMappingAction().Process(src, dest, ctx);
        return this;
    }

    /// <summary>
    /// Imposta un'azione da eseguire dopo il mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest> afterAction)
    {
        AfterMapAction = afterAction;
        return this;
    }

    /// <summary>
    /// Imposta un'azione da eseguire dopo il mapping, con contesto.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest, ResolutionContext> afterAction)
    {
        AfterMapActionWithContext = afterAction;
        return this;
    }

    /// <summary>
    /// Imposta un'azione di mapping personalizzata da eseguire dopo il mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new()
    {
        AfterMapActionWithContext = (src, dest, ctx) => new TMappingAction().Process(src, dest, ctx);
        return this;
    }

    /// <summary>
    /// Usa un converter personalizzato per la mappatura.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(ITypeConverter<TSource, TDest> converter)
    {
        TypeConverter = converter;
        return this;
    }

    /// <summary>
    /// Usa una funzione personalizzata per la mappatura.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(Func<TSource, TDest, ResolutionContext, TDest> mappingFunction)
    {
        CustomMappingFunction = mappingFunction;
        return this;
    }

    /// <summary>
    /// Usa un'espressione personalizzata per la mappatura.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(Expression<Func<TSource, TDest>> mappingExpression)
    {
        CustomMappingExpression = mappingExpression;
        return this;
    }

    /// <summary>
    /// Usa un costruttore personalizzato per creare l'oggetto di destinazione.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, TDest> ctor)
    {
        CustomCtor = ctor;
        return this;
    }

    /// <summary>
    /// Usa un costruttore personalizzato con contesto per creare l'oggetto di destinazione.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, ResolutionContext, TDest> ctor)
    {
        CustomCtorWithContext = ctor;
        return this;
    }

    /// <summary>
    /// Imposta la profondità massima per il mapping ricorsivo.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> MaxDepth(int depth)
    {
        MaxDepthValue = depth;
        return this;
    }

    /// <summary>
    /// Abilita la preservazione dei riferimenti durante il mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> PreserveReferences()
    {
        PreserveReferencesValue = true;
        return this;
    }

    /// <summary>
    /// Disabilita la validazione del costruttore.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> DisableCtorValidation()
    {
        DisableCtorValidationValue = true;
        return this;
    }

    /// <summary>
    /// Imposta la modalità di validazione dei membri.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ValidateMemberList(MemberList memberList)
    {
        MemberListValidation = memberList;
        return this;
    }

    /// <summary>
    /// Include tutti i tipi derivati nel mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> IncludeAllDerived()
    {
        IncludeAllDerivedValue = true;
        return this;
    }

    /// <summary>
    /// Include un tipo derivato specifico nel mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> Include(Type derivedSourceType, Type derivedDestinationType)
    {
        Includes.Add((derivedSourceType, derivedDestinationType));
        return this;
    }

    /// <summary>
    /// Include un tipo base specifico nel mapping.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> IncludeBase(Type sourceBase, Type destinationBase)
    {
        IncludeBases.Add((sourceBase, destinationBase));
        return this;
    }

    /// <summary>
    /// Ignora tutte le proprietà di destinazione con setter non accessibili.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> IgnoreAllPropertiesWithAnInaccessibleSetter()
    {
        IgnoreInaccessibleSetter = true;
        return this;
    }

    /// <summary>
    /// Ignora tutte le proprietà sorgente con setter non accessibili.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
    {
        IgnoreSourceInaccessibleSetter = true;
        return this;
    }

    /// <summary>
    /// Configura le opzioni per un parametro del costruttore.
    /// </summary>
    public ISolutionMappingExpression<TSource, TDest> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
    {
        CtorParamConfigs.Add((ctorParamName, paramOptions));
        return this;
    }

    /// <summary>
    /// Crea una configurazione inversa di mapping (ReverseMap) tra TDest e TSource.
    /// </summary>
    public SolutionMappingExpression<TDest, TSource> ReverseMap()
    {
        var reverse = new SolutionMappingExpression<TDest, TSource>();

        // Copia i membri mappati e le proprietà ignorate
        foreach (var (destProp, getter, valueConverter) in Members)
        {
            var srcProp = typeof(TSource).GetProperty(destProp.Name);
            var destPropReverse = typeof(TDest).GetProperty(destProp.Name);
            if (srcProp != null && destPropReverse != null)
            {
                reverse.Members.Add((destPropReverse, d => srcProp.GetValue(d), valueConverter));
            }
        }
        foreach (var ignored in IgnoredMembers)
            reverse.IgnoredMembers.Add(ignored);

        // Copia le altre configurazioni rilevanti
        reverse.BeforeMapAction = null;
        reverse.AfterMapAction = null;
        reverse.TypeConverter = null;
        reverse.CustomMappingFunction = null;
        reverse.CustomMappingExpression = null;
        reverse.CustomCtor = null;
        reverse.CustomCtorWithContext = null;
        reverse.MaxDepthValue = MaxDepthValue;
        reverse.PreserveReferencesValue = PreserveReferencesValue;
        reverse.DisableCtorValidationValue = DisableCtorValidationValue;
        reverse.MemberListValidation = MemberListValidation;
        reverse.IncludeAllDerivedValue = IncludeAllDerivedValue;
        reverse.Includes.AddRange(Includes);
        reverse.IncludeBases.AddRange(IncludeBases);
        reverse.IgnoreInaccessibleSetter = IgnoreInaccessibleSetter;
        reverse.IgnoreSourceInaccessibleSetter = IgnoreSourceInaccessibleSetter;
        foreach (var vt in ValueTransformers)
            reverse.ValueTransformers.Add(vt);

        return reverse;
    }

    /// <summary>
    /// Crea una configurazione inversa di mapping (ReverseMap) e la aggiunge al profilo specificato.
    /// </summary>
    public SolutionMappingExpression<TDest, TSource> ReverseMap(SolutionMapperProfile profile)
    {
        var reverse = new SolutionMappingExpression<TDest, TSource>();

        // Copia i membri mappati e le proprietà ignorate
        foreach (var (destProp, getter, valueConverter) in Members)
        {
            var srcProp = typeof(TSource).GetProperty(destProp.Name);
            var destPropReverse = typeof(TDest).GetProperty(destProp.Name);
            if (srcProp != null && destPropReverse != null)
            {
                reverse.Members.Add((destPropReverse, d => srcProp.GetValue(d), valueConverter));
            }
        }
        foreach (var ignored in IgnoredMembers)
            reverse.IgnoredMembers.Add(ignored);

        // Copia le altre configurazioni rilevanti
        reverse.BeforeMapAction = null;
        reverse.AfterMapAction = null;
        reverse.TypeConverter = null;
        reverse.CustomMappingFunction = null;
        reverse.CustomMappingExpression = null;
        reverse.CustomCtor = null;
        reverse.CustomCtorWithContext = null;
        reverse.MaxDepthValue = MaxDepthValue;
        reverse.PreserveReferencesValue = PreserveReferencesValue;
        reverse.DisableCtorValidationValue = DisableCtorValidationValue;
        reverse.MemberListValidation = MemberListValidation;
        reverse.IncludeAllDerivedValue = IncludeAllDerivedValue;
        reverse.Includes.AddRange(Includes);
        reverse.IncludeBases.AddRange(IncludeBases);
        reverse.IgnoreInaccessibleSetter = IgnoreInaccessibleSetter;
        reverse.IgnoreSourceInaccessibleSetter = IgnoreSourceInaccessibleSetter;
        foreach (var vt in ValueTransformers)
            reverse.ValueTransformers.Add(vt);

        profile.Rules.Add(reverse);
        return reverse;
    }
}