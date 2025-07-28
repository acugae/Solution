namespace Solution.SolutionMapper;

public interface ITypeConverter<TSource, TDest>
{
    TDest Convert(TSource source, TDest destination, ResolutionContext context);
}