using System;
using System.Collections.Generic;

namespace HalloDoc.Models;

public partial class AspNetUser
{
    public string Id { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Ip { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Admin> AdminAspNetUsers { get; set; } = new List<Admin>();

    public virtual ICollection<Admin> AdminModifiedByNavigations { get; set; } = new List<Admin>();

    public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; } = new List<AspNetUserRole>();

    public virtual ICollection<Business> BusinessCreatedByNavigations { get; set; } = new List<Business>();

    public virtual ICollection<Business> BusinessModifiedByNavigations { get; set; } = new List<Business>();

    public virtual ICollection<Physician> PhysicianAspNetUsers { get; set; } = new List<Physician>();

    public virtual ICollection<Physician> PhysicianCreatedByNavigations { get; set; } = new List<Physician>();

    public virtual ICollection<Physician> PhysicianModifiedByNavigations { get; set; } = new List<Physician>();

    public virtual ICollection<ShiftDetail> ShiftDetails { get; set; } = new List<ShiftDetail>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
