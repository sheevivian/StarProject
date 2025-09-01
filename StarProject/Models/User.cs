using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class User
{
    public string No { get; set; } = null!;

    public string Account { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PasswordSalt { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string IdNumber { get; set; } = null!;

    public byte Status { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<CustomerService> CustomerServices { get; set; } = new List<CustomerService>();

    public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();

    public virtual ICollection<OrderC> OrderCs { get; set; } = new List<OrderC>();

    public virtual ICollection<OrderMaster> OrderMasters { get; set; } = new List<OrderMaster>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
