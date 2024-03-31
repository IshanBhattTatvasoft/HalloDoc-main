using HalloDoc.DataLayer.Models;
using HalloDoc.LogicLayer.Patient_Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using HalloDoc.DataLayer.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;



namespace HalloDoc.LogicLayer.Patient_Repository
{
    public class JwtToken : IJwtToken
    {

        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;

        public JwtToken(IConfiguration configuration, ApplicationDbContext context, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _context = context;
            _contextAccessor = contextAccessor;
        }

        public string GenerateJwtToken(AspNetUser user)
        {
            AspNetUserRole anur = _context.AspNetUserRoles.Where(a => a.UserId == user.Id).FirstOrDefault();
            Admin ad = _context.Admins.Where(a => a.AspNetUserId == user.Id).FirstOrDefault();
            AspNetRole anr = _context.AspNetRoles.Where(b => b.Id == anur.RoleId).FirstOrDefault();
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, anr.Name),
                new Claim("userId", user.Id.ToString()),
                new Claim("roleId", anur.RoleId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(20);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        
        public bool ValidateToken(string token, out JwtSecurityToken jwtSecurityToken)
        {
            jwtSecurityToken = null!;

            if (token == null)
            {
                return false;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                }, out SecurityToken validatedToken);

                jwtSecurityToken = (JwtSecurityToken)validatedToken;
                var x = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "roleId").Value;
                if (jwtSecurityToken != null)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetRoleId(string token)
        {
            string roleId = "";

            if (token == null)
            {
                return roleId;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)validatedToken;
                var x = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "roleId").Value;
                return x;
            }
            catch
            {
                return roleId;
            }
        }
    }
}
