using System.Linq.Expressions;

namespace Solution.SolutionMapper;

public interface ISolutionMappingExpression<TSource, TDest>
{
    ISolutionMappingExpression<TSource, TDest> ForMember<TMember>(
        Expression<Func<TDest, TMember>> destSelector,
        Expression<Func<TSource, TMember>> srcSelector,
        IValueConverter<TMember, TMember> converter = null);

    ISolutionMappingExpression<TSource, TDest> ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);

    ISolutionMappingExpression<TSource, TDest> Ignore(Expression<Func<TDest, object>> destSelector);
    ISolutionMappingExpression<TSource, TDest> Ignore(string field);

    ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest> beforeAction);
    ISolutionMappingExpression<TSource, TDest> BeforeMap(Action<TSource, TDest, ResolutionContext> beforeAction);
    ISolutionMappingExpression<TSource, TDest> BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new();

    ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest> afterAction);
    ISolutionMappingExpression<TSource, TDest> AfterMap(Action<TSource, TDest, ResolutionContext> afterAction);
    ISolutionMappingExpression<TSource, TDest> AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDest>, new();

    ISolutionMappingExpression<TSource, TDest> ConvertUsing(ITypeConverter<TSource, TDest> converter);
    ISolutionMappingExpression<TSource, TDest> ConvertUsing(Func<TSource, TDest, ResolutionContext, TDest> mappingFunction);
    ISolutionMappingExpression<TSource, TDest> ConvertUsing(Expression<Func<TSource, TDest>> mappingExpression);

    ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, TDest> ctor);
    ISolutionMappingExpression<TSource, TDest> ConstructUsing(Func<TSource, ResolutionContext, TDest> ctor);

    ISolutionMappingExpression<TSource, TDest> MaxDepth(int depth);
    ISolutionMappingExpression<TSource, TDest> PreserveReferences();
    ISolutionMappingExpression<TSource, TDest> DisableCtorValidation();

    ISolutionMappingExpression<TSource, TDest> ValidateMemberList(MemberList memberList);

    ISolutionMappingExpression<TSource, TDest> IncludeAllDerived();
    ISolutionMappingExpression<TSource, TDest> Include(Type derivedSourceType, Type derivedDestinationType);
    ISolutionMappingExpression<TSource, TDest> IncludeBase(Type sourceBase, Type destinationBase);

    ISolutionMappingExpression<TSource, TDest> IgnoreAllPropertiesWithAnInaccessibleSetter();
    ISolutionMappingExpression<TSource, TDest> IgnoreAllSourcePropertiesWithAnInaccessibleSetter();

    ISolutionMappingExpression<TSource, TDest> ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions);

    IList<ValueTransformerConfiguration> ValueTransformers { get; }

    SolutionMappingExpression<TDest, TSource> ReverseMap(SolutionMapperProfile profile);
}