namespace Solution.Infrastructure.Models;
public class cUser
{
    public cUser() { }
    public cUser(IEnumerable<Claim> oClames)
    {
        foreach (var clame in oClames)
        {
            switch (clame.Type)
            {
                case "Id":
                    Id = clame.Value;
                    break;
                case "UserName":
                    UserName = clame.Value;
                    break;
                case "FullName":
                    FullName = clame.Value;
                    break;
                default:
                    break;
            }
        }
    }
    public string Id { get; set; }
    public string Domain { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public List<string> Groups { get; set; } = [];
    public Claim[] GetClaims()
    {
        var claims = new[] {
                        new Claim("Id", Id),
                        new Claim("Domain", Domain),
                        new Claim("UserName", UserName),
                        new Claim("Groups", String.Join(",", Groups))
                    };
        return claims;
    }
}
