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
            ApplicationDbContext _context = new ApplicationDbContext();

            var jwtService = context.HttpContext.RequestServices.GetService<IJwtToken>();
            var adminInterface = context.HttpContext.RequestServices.GetService<IAdminInterface>();
            if (jwtService == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));

                return;
            }
            var request = context.HttpContext.Request;
            var token = request.Cookies["token"];

            HttpRequest request1 = context.HttpContext.Request;

            if (token == null || !jwtService.ValidateToken(token, out JwtSecurityToken jwtToken))
            {
                if (isAjaxRequest(request1))                {                    context.Result = new JsonResult(new { error = "Failed to Authenticate User" })                    {                        StatusCode = 401                    };                }                else                {                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Login", action = "PatientLoginPage", }));                }                return;
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
            string userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId").Value;
            string patientUserId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId").Value;

            string roleId = _context.AspNetUserRoles.FirstOrDefault(a => a.UserId == Convert.ToInt32(userId)).RoleId.ToString();

            List<string> allMenus = adminInterface.GetAllMenus(roleIdVal);

            bool isHavingAccess = false;

           

            if (_menu != null)
            {
                if (roleId == "3" || allMenus.Any(r => r == _menu))
                {
                    isHavingAccess = true;
                }
            }

            if (!isHavingAccess)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Admin", action = "PageNotFoundError", }));
                return;
            }

            if(roleId == "2")
            {
                var id = context.RouteData.Values["id"];
                if(id!=null)
                {
                    Physician p = _context.Physicians.FirstOrDefault(pe => pe.AspNetUserId == Convert.ToInt32(userId));
                    Request r = _context.Requests.FirstOrDefault(re => re.RequestId == Convert.ToInt32(id));
                    if(r!=null && p.PhysicianId != r.PhysicianId)
                    {
                        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Admin", action = "PageNotFoundError", }));
                        return;
                    }
                }
            }

            if(roleId == "3")
            {
                var id = context.RouteData.Values["id"];
                if(id!=null)
                {
                    User u = _context.Users.FirstOrDefault(us => us.UserId == Convert.ToInt32(patientUserId));
                    Request r = _context.Requests.FirstOrDefault(re => re.RequestId == Convert.ToInt32(id) && re.UserId == Convert.ToInt32(patientUserId));
                    if(r!=null && r.UserId!=u.UserId)
                    {
                        context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Admin", action = "PageNotFoundError", }));
                        return;
                    }
                }
            }
        }

        private bool isAjaxRequest(HttpRequest request)        {            return request.Headers["X-Requested-With"] == "XMLHttpRequest";        }
    }


}

