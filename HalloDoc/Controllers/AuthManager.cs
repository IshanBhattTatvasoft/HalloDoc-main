using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using HalloDoc.DataLayer.Models;
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

        public CustomAuthorize(string role="")
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var jwtService = context.HttpContext.RequestServices.GetService<IJwtToken>();
            if (jwtService == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Patient", action = "Index", }));
                return;
            }
            var request = context.HttpContext.Request;
            var token = request.Cookies["token"];

            if (token == null || !jwtService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Patient", action = "Index", }));
                return;
            }

            var roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Patient", action = "Index", }));
                return;
            }
            if (string.IsNullOrWhiteSpace(_role) || roleClaim.Value != _role)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Patient", action = "Index", }));
                return;
            }
            if (roleClaim!.Value != _role)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home", action = "PatientLogin" }));
                return;
            }
        }

        
    }


}

