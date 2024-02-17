namespace Solution.Infrastructure.Models;
public class Route
{
    public int ID { get; set; }
    public string Pattern { get; set; }
    public string Assembly { get; set; }
    public string Class { get; set; }
    public string Function { get; set; }
    public string httpMethods { get; set; }
}
