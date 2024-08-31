using BookManagementPortalProxy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;

namespace BookManagementPortalProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapnaBookHouseLoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private IConfiguration _configuration;
        private readonly ILogger<SapnaBookHouseLoginController> _logger;

        public SapnaBookHouseLoginController(ApplicationDbContext context, IConfiguration configuration, ILogger<SapnaBookHouseLoginController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        private Credentials AuthenticateUser(Credentials credentials)   
        {
            _logger.LogInformation("Authetication of user started");
            Credentials _credentials = null;

            if (credentials.UserName == "SapnaBookHouse" && credentials.Password == "BookHouse@123")
            {
                _credentials = new Credentials { UserName = "SapanaBook" };
                _logger.LogInformation("Authetication done successfully");
            }

            _logger.LogInformation("Completed Authenticated method");
            return _credentials;
        }

        private string GenerateAccessToken(Credentials credentials)
        {
            _logger.LogInformation("Generate access token method started");
            _logger.LogDebug("Token generation started for {UserName}", credentials.UserName);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signin = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, credentials.UserName)
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"], claims, expires:  DateTime.UtcNow.AddMinutes(5),
                signingCredentials: signin);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return refreshToken;
        }

        private void SaveRefreshToken(string username, string refreshToken)
        {
            var token = new RefreshToken
            {
                UserName = username,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
        }

        private bool ValidateRefreshToken(string username, string refreshToken)
        {
            var token = _context.RefreshTokens
                .FirstOrDefault(token => token.UserName == username && token.Token == refreshToken);

            if (token == null || token.ExpiresAt <= DateTime.UtcNow) 
                return false;

            return true;
        }

        private void RevokeRefreshToken(string username, string refreshToken)
        {
            var token = _context.RefreshTokens
                .FirstOrDefault(rt => rt.UserName == username && rt.Token == refreshToken);

            if (token != null)
            {
                _context.RefreshTokens.Remove(token);
                _context.SaveChanges();
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(Credentials credentials)
        {
            _logger.LogInformation("Login method started");

            IActionResult response = Unauthorized();

            var user = AuthenticateUser(credentials);

            if (user != null)
            {
                _logger.LogDebug("Authenticated {UserName}", credentials.UserName);
                var accesstoken = GenerateAccessToken(user);
                var refreshtoken = GenerateRefreshToken();
                SaveRefreshToken(user.UserName, refreshtoken);
                response = Ok(new TokenResponse
                {
                    AccessToken = accesstoken,
                    RefreshToken = refreshtoken
                });
                _logger.LogDebug("Token genearted and sent to {UserName}", credentials.UserName);
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh(TokenResponse tokenResponse)
        {
            var principal = GetPrincipalFromExpiredToken(tokenResponse.AccessToken);
            var username = principal.Identity.Name;

            if (!ValidateRefreshToken(username, tokenResponse.RefreshToken))
            {
                return Unauthorized("Invalid Refresh Token");
            }

            var newaccesstoken = GenerateAccessToken(new Credentials { UserName = username});
            var newrefreshtoken = GenerateRefreshToken();
            RevokeRefreshToken(username, tokenResponse.RefreshToken);
            SaveRefreshToken(username, newrefreshtoken);

            return Ok(new TokenResponse
            {
                AccessToken = newaccesstoken,
                RefreshToken = newrefreshtoken
            });
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }
            return principal;
        }

        //private string GenerateToken(Credentials credentials)
        //{
        //    _logger.LogInformation("Generate token method started");

        //    _logger.LogDebug("Token generation started for {UserName}", credentials.UserName);

        //    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var signinDetails = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, credentials.UserName)
        //    };

        //    _logger.LogDebug($"Generating token with claims {claims}");

        //    var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"],
        //        claims, expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: signinDetails);

        //    _logger.LogInformation("Token generated successfully");

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}
    }
}
