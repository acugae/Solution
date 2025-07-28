namespace Solution.SolutionMapper;

public class ResolutionContext
{
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    public Dictionary<object, object> InstanceCache { get; } = new();
    public int MaxDepth { get; set; } = 10;
}