namespace Solution.SolutionMapper;

public interface IValueConverter<TSourceMember, TDestMember>
{
    TDestMember Convert(TSourceMember sourceMember, TDestMember destination, ResolutionContext context);
}