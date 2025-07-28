using Solution.SolutionMapper;

namespace Solution.SolutionMapper.Extensions;

public class SolutionMapperConfigurationExpression
{
    private readonly SolutionMapper _mapper;

    public SolutionMapperConfigurationExpression(SolutionMapper mapper)
    {
        _mapper = mapper;
    }

    public SolutionMappingExpression<TSource, TDest> CreateMap<TSource, TDest>()
    {
        var expr = new SolutionMappingExpression<TSource, TDest>();
        _mapper.AddMapping(expr);
        return expr;
    }

    public void AddTypeConverter<TSource, TDest>(ITypeConverter<TSource, TDest> converter)
    {
        _mapper.AddTypeConverter(converter);
    }
}