using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

public class JwtHelper
{
    private readonly IConfiguration _config;
    public JwtHelper(IConfiguration configuration)
    {
        _config = configuration;
    }
    public string GenerateJwtToken(int employeeId, string userType, string Username, bool IsLoginSuccessful)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Id", employeeId.ToString()),
                new Claim("Type",userType),
                new Claim("Username",Username),
                new Claim("IsLoginSuccessful", IsLoginSuccessful.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            //        var claims = new List<Claim> {
            //    new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()),
            //    new Claim("Role", role)
            //};

            //var token = new JwtSecurityToken(
            //    issuer: _config["Jwt:Issuer"],
            //    audience: "yourdomain.com",
            //    claims: claims,
            //    expires: DateTime.Now.AddHours(1),
            //    signingCredentials: credentials);

            //return new JwtSecurityTokenHandler().WriteToken(token);
            var jwtToken = new JwtSecurityToken(
       claims: claims,
       notBefore: DateTime.UtcNow,
       expires: DateTime.UtcNow.AddDays(30),
       signingCredentials: new SigningCredentials(
           new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(_config["Jwt:Key"])
               ),
           SecurityAlgorithms.HmacSha256Signature)
       );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}