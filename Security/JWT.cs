using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Solution.Security;
public class JWT
{
    /// <summary>
    /// Legge e valida un token JWT (validazione base per retrocompatibilità).
    /// Per una validazione completa usare ReadSecure().
    /// </summary>
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
    
    /// <summary>
    /// Legge e valida un token JWT con validazione completa (RACCOMANDATO).
    /// </summary>
    /// <param name="key">Chiave segreta per validare la firma</param>
    /// <param name="token">Token JWT da validare</param>
    /// <param name="validIssuer">Issuer atteso (opzionale)</param>
    /// <param name="validAudience">Audience atteso (opzionale)</param>
    /// <returns>Claims contenuti nel token</returns>
    /// <exception cref="SecurityTokenException">Token non valido</exception>
    public static Claim[] ReadSecure(string key, string token, string? validIssuer = null, string? validAudience = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        if (string.IsNullOrEmpty(token))
            throw new ArgumentNullException(nameof(token));
        
        // Verifica lunghezza minima chiave (almeno 32 caratteri per HMAC-SHA256)
        if (key.Length < 32)
            throw new ArgumentException("Key must be at least 32 characters for security", nameof(key));
            
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(validIssuer),
            ValidIssuer = validIssuer,
            ValidateAudience = !string.IsNullOrEmpty(validAudience),
            ValidAudience = validAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuerSigningKey = true,  // SEMPRE validare la firma
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1) // Tolleranza minima per clock skew
        };
        
        tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
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
    
    /// <summary>
    /// Crea un token JWT con issuer e audience (RACCOMANDATO).
    /// </summary>
    /// <param name="key">Chiave segreta (minimo 32 caratteri)</param>
    /// <param name="expirationMinutes">Minuti di validità del token</param>
    /// <param name="claims">Claims da includere nel token</param>
    /// <param name="issuer">Issuer del token</param>
    /// <param name="audience">Audience del token</param>
    /// <param name="algorithm">Algoritmo di firma (default: HmacSha256)</param>
    /// <returns>Token JWT firmato</returns>
    public static string CreateSecure(string key, int expirationMinutes, Claim[] claims, 
        string issuer, string audience, string algorithm = SecurityAlgorithms.HmacSha256)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        if (key.Length < 32)
            throw new ArgumentException("Key must be at least 32 characters for security", nameof(key));
        if (expirationMinutes <= 0)
            throw new ArgumentException("Expiration must be positive", nameof(expirationMinutes));
        if (string.IsNullOrEmpty(issuer))
            throw new ArgumentNullException(nameof(issuer));
        if (string.IsNullOrEmpty(audience))
            throw new ArgumentNullException(nameof(audience));
            
        var allClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };
        
        if (claims != null)
            allClaims.AddRange(claims);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, algorithm);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: allClaims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
