using System.Linq.Expressions;
using System.Reflection;

namespace Solution.SolutionMapper;
public class SolutionMappingExpression<TSource, TDest> : ISolutionMappingExpression<TSource, TDest>
{
    internal List<(PropertyInfo destProp, Func<TSource, object> getter, object valueConverter)> Members { get; } = new();
    internal HashSet<string> IgnoredMembers { get; } = new();
    internal List<(string, Action<ISourceMemberConfigurationExpression>)> SourceMemberConfigs { get; } = new();
    internal List<(string, Action<ICtorParamConfigurationExpression<TSource>>)> CtorParamConfigs { get; } = new();
    internal Action<TSource, TDest> BeforeMapAction { get; private set; }
    internal Action<TSource, TDest, ResolutionContext> BeforeMapActionWithContext { get; private set; }
    internal Action<TSource, TDest> AfterMapAction { get; private set; }
    internal Action<TSource, TDest, ResolutionContext> AfterMapActionWithContext { get; private set; }
    internal ITypeConverter<TSource, TDest> TypeConverter { get; private set; }
    internal Func<TSource, TDest, ResolutionContext, TDest> CustomMappingFunction { get; private set; }
    internal Expression<Func<TSource, TDest>> CustomMappingExpression { get; private set; }
    internal Func<TSource, TDest> CustomCtor { get; private set; }
    internal Func<TSource, ResolutionContext, TDest> CustomCtorWithContext { get; private set; }
    internal int? MaxDepthValue { get; private set; }
    internal bool PreserveReferencesValue { get; private set; }
    internal bool DisableCtorValidationValue { get; private set; }
    internal MemberList? MemberListValidation { get; private set; }
    internal bool IncludeAllDerivedValue { get; private set; }
    internal List<(Type, Type)> Includes { get; } = new();
    internal List<(Type, Type)> IncludeBases { get; } = new();
    internal bool IgnoreInaccessibleSetter { get; private set; }
    internal bool IgnoreSourceInaccessibleSetter { get; private set; }
    public IList<ValueTransformerConfiguration> ValueTransformers { get; } = new List<ValueTransformerConfiguration>();

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

    public ISolutionMappingExpression<TSource, TDest> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        SourceMemberConfigs.Add((sourceMemberName, memberOptions));
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> Ignore(string field)
    {
        IgnoredMembers.Add(field);
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> Ignore(Expression<Func<TDest, object>> destSelector)
    {
        var destProp = (PropertyInfo)((MemberExpression)(destSelector.Body is UnaryExpression ue ? ue.Operand : destSelector.Body)).Member;
        IgnoredMembers.Add(destProp.Name);
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest> beforeAction)
    {
        BeforeMapAction = beforeAction;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest, ResolutionContext> beforeAction)
    {
        BeforeMapActionWithContext = beforeAction;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new()
    {
        BeforeMapActionWithContext = (src, dest, ctx) => new TMappingAction().Process(src, dest, ctx);
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest> afterAction)
    {
        AfterMapAction = afterAction;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest, ResolutionContext> afterAction)
    {
        AfterMapActionWithContext = afterAction;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new()
    {
        AfterMapActionWithContext = (src, dest, ctx) => new TMappingAction().Process(src, dest, ctx);
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(ITypeConverter<TSource, TDest> converter)
    {
        TypeConverter = converter;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(Func<TSource, TDest, ResolutionContext, TDest> mappingFunction)
    {
        CustomMappingFunction = mappingFunction;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ConvertUsing(Expression<Func<TSource, TDest>> mappingExpression)
    {
        CustomMappingExpression = mappingExpression;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, TDest> ctor)
    {
        CustomCtor = ctor;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, ResolutionContext, TDest> ctor)
    {
        CustomCtorWithContext = ctor;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> MaxDepth(int depth)
    {
        MaxDepthValue = depth;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> PreserveReferences()
    {
        PreserveReferencesValue = true;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> DisableCtorValidation()
    {
        DisableCtorValidationValue = true;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ValidateMemberList(MemberList memberList)
    {
        MemberListValidation = memberList;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> IncludeAllDerived()
    {
        IncludeAllDerivedValue = true;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> Include(Type derivedSourceType, Type derivedDestinationType)
    {
        Includes.Add((derivedSourceType, derivedDestinationType));
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> IncludeBase(Type sourceBase, Type destinationBase)
    {
        IncludeBases.Add((sourceBase, destinationBase));
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> IgnoreAllPropertiesWithAnInaccessibleSetter()
    {
        IgnoreInaccessibleSetter = true;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
    {
        IgnoreSourceInaccessibleSetter = true;
        return this;
    }

    public ISolutionMappingExpression<TSource, TDest> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
    {
        CtorParamConfigs.Add((ctorParamName, paramOptions));
        return this;
    }

    // ReverseMap: crea una configurazione inversa
    public SolutionMappingExpression<TDest, TSource> ReverseMap()
    {
        var reverse = new SolutionMappingExpression<TDest, TSource>();

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

        // Copia altre configurazioni se necessario
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

    // ReverseMap: crea una configurazione inversa
    public SolutionMappingExpression<TDest, TSource> ReverseMap(SolutionMapperProfile profile)
    {
        var reverse = new SolutionMappingExpression<TDest, TSource>();

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

        // Copia altre configurazioni se necessario
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