using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.util;

namespace Solution.Security;
public class JWT
{
    private string _Audience;
    private string _Issuer;
    private string _Key;
    private string _Subject;
    public JWT(string Audience, string Issuer, string Key, string Subject) {
        _Audience = Audience;
        _Issuer = Issuer;
        _Key = Key;
        _Subject = Subject;
    }
    //public Claim[] readTokenByHeader(HttpContext context)
    //{
    //    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    tokenHandler.ValidateToken(token, new TokenValidationParameters
    //    {
    //        ValidateIssuer = true,
    //        ValidateAudience = true,
    //        ValidAudience = _Audience,
    //        ValidIssuer = _Issuer,
    //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Key))
    //    }, out SecurityToken validatedToken);
    //    var jwtToken = (JwtSecurityToken)validatedToken;
    //    context.Items["User"] = new User(jwtToken.Claims);
    //}
    public Claim[] Read(string sToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(sToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = _Audience,
            ValidIssuer = _Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Key))
        }, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken.Claims.ToArray();
    }
    public string Create(Claim[] addClaims)
    {
        List<Claim> claims = [
                        new Claim(JwtRegisteredClaimNames.Sub, _Subject),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        .. addClaims.ToArray(),
                    ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_Key));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _Issuer,
            _Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signIn);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
