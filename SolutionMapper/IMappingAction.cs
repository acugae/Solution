namespace Solution.SolutionMapper;

public interface IMappingAction<TSource, TDest>
{
    void Process(TSource source, TDest destination, ResolutionContext context);
}