using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Emp
{
    public string No { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int RoleNo { get; set; }

    public int DeptNo { get; set; }

    public DateTime HireDate { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public string EmpCode { get; set; } = null!;

    public bool Status { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CustomerService> CustomerServices { get; set; } = new List<CustomerService>();

    public virtual Dept DeptNoNavigation { get; set; } = null!;

    public virtual ICollection<OrderC> OrderCs { get; set; } = new List<OrderC>();

    public virtual Role RoleNoNavigation { get; set; } = null!;
}
