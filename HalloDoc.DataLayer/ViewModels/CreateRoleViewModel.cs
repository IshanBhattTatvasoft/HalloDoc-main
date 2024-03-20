using HalloDoc.DataLayer.Models;
using System;
using System.Collections.Generic;
using HalloDoc.DataLayer.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace HalloDoc.DataLayer.ViewModels;

public class CreateRoleViewModel
{
    public AdminNavbarModel? adminNavbarModel { get; set; }
    [Required(ErrorMessage = "Role name is required")]
    public string roleName { get; set; }
    public List<Menu> allRoles { get; set; }
    public List<Role> roles { get; set; }
    public string? NameOfRole { get; set; }
}

