using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Emp
{
    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int DeptNo { get; set; }

    public DateTime HireDate { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public bool Status { get; set; }

    public int? RoleNo { get; set; }

    public bool ForceChangePassword { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? IdNumber { get; set; }

    public DateTime? BirthDate { get; set; }

    public DateTime? LastLogin { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CustomerService> CustomerServices { get; set; } = new List<CustomerService>();

    public virtual ICollection<OrderC> OrderCs { get; set; } = new List<OrderC>();
}
