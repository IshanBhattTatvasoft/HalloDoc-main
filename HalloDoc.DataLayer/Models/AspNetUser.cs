using System;
using System.Collections.Generic;

namespace HalloDoc.DataLayer.Models;

public partial class AspNetUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Ip { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Admin> AdminAspNetUsers { get; set; } = new List<Admin>();

    public virtual ICollection<Admin> AdminCreatedByNavigations { get; set; } = new List<Admin>();

    public virtual ICollection<Admin> AdminModifiedByNavigations { get; set; } = new List<Admin>();

    public virtual ICollection<Business> BusinessCreatedByNavigations { get; set; } = new List<Business>();

    public virtual ICollection<Business> BusinessModifiedByNavigations { get; set; } = new List<Business>();

    public virtual ICollection<PasswordReset> PasswordResets { get; set; } = new List<PasswordReset>();

    public virtual ICollection<Physician> PhysicianAspNetUsers { get; set; } = new List<Physician>();

    public virtual ICollection<Physician> PhysicianCreatedByNavigations { get; set; } = new List<Physician>();

    public virtual ICollection<Physician> PhysicianModifiedByNavigations { get; set; } = new List<Physician>();

    public virtual ICollection<RequestNote> RequestNoteCreatedByNavigations { get; set; } = new List<RequestNote>();

    public virtual ICollection<RequestNote> RequestNoteModifiedByNavigations { get; set; } = new List<RequestNote>();

    public virtual ICollection<ShiftDetail> ShiftDetails { get; set; } = new List<ShiftDetail>();

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();

    public virtual ICollection<User> UserAspNetUsers { get; set; } = new List<User>();

    public virtual ICollection<User> UserCreatedByNavigations { get; set; } = new List<User>();

    public virtual ICollection<User> UserModifiedByNavigations { get; set; } = new List<User>();

    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
