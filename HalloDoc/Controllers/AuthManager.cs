using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using HalloDoc.DataLayer.Models;
using HalloDoc.DataLayer.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using HalloDoc.LogicLayer.Patient_Interface;
using System.Security.Claims;

namespace HalloDoc.Controllers
{
    partial interface IAuthManager
    {
        public AspNetUser Login(string username, string password);
    }

    public class AuthManager : IAuthManager
    {
        public AspNetUser Login(string username, string password)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            AspNetUser au = context.AspNetUsers.Where(u => u.UserName == username && u.PasswordHash == password).FirstOrDefault();
            return au;
        }
    }

    public class CustomAuthorize : Attribute, IAuthorizationFilter
    {
        private readonly string _role;
        private readonly string _menu;

        public CustomAuthorize(string role = "", string menu = "")
        {
            _role = role;
            _menu = menu;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var jwtService = context.HttpContext.RequestServices.GetService<IJwtToken>();
            var adminInterface = context.HttpContext.RequestServices.GetService<IAdminInterface>();
            if (jwtService == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));

                return;
            }
            var request = context.HttpContext.Request;
            var token = request.Cookies["token"];

            if (token == null || !jwtService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));

                return;
            }

            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));

                return;
            }
            if (string.IsNullOrWhiteSpace(_role) || !_role.Contains(roleClaim.Value))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));

                return;
            }

            string roleIdVal = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "roleId").Value;

            List<string> allMenus = adminInterface.GetAllMenus(roleIdVal);

            bool isHavingAccess = false;

            if (_menu != null)
            {
                if (roleIdVal=="3" || allMenus.Any(r => r == _menu))
                {
                    isHavingAccess = true;
                }
            }

            if (!isHavingAccess)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Admin", action = "PageNotFoundError", }));
                return;
            }
        }


    }


}

