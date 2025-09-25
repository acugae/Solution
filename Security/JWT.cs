using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Solution.Security;
public class JWT
{
    public static Claim[] Read(string Key, string sToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            ValidateIssuerSigningKey = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        tokenHandler.ValidateToken(sToken, validationParameters, out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        return jwtToken.Claims.ToArray();
    }
    public static string Create(string Key, int expiderMinutes, Claim[] addClaims, string Algorithm = SecurityAlgorithms.HmacSha256)
    {
        List<Claim> claims = [
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        new Claim(JwtRegisteredClaimNames.Exp, ((DateTimeOffset)DateTime.UtcNow.AddMinutes(expiderMinutes)).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        .. addClaims.ToArray(),
                    ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var signIn = new SigningCredentials(key, Algorithm);
        var token = new JwtSecurityToken(null, null,
            claims,
            signingCredentials: signIn);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
