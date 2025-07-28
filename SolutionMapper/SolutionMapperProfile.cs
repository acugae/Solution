namespace Solution.SolutionMapper;
public abstract class SolutionMapperProfile
{
    protected SolutionMapper Mapper { get; }

    protected SolutionMapperProfile(SolutionMapper mapper)
    {
        Mapper = mapper;
    }

    internal List<object> Rules { get; } = new();

    protected ISolutionMappingExpression<TSource, TDest> CreateMap<TSource, TDest>()
    {
        var rule = new SolutionMappingExpression<TSource, TDest>();
        Rules.Add(rule);
        return rule;
    }

    protected void AddReverseMap<TSource, TDest>(SolutionMappingExpression<TSource, TDest> mapping)
    {
        var reverse = mapping.ReverseMap();
        Rules.Add(reverse);
    }
}