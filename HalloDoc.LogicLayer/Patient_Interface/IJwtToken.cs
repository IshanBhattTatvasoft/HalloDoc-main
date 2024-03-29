using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HalloDoc.LogicLayer.Patient_Interface
{
    public interface IJwtToken
    {
        public string GenerateJwtToken(AspNetUser user);
        public bool ValidateToken(string token, out JwtSecurityToken jwtSecurityToken);
        public string GetRoleId(string token);

    }
}
