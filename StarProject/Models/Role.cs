using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Role
{
    public int No { get; set; }

    public string RoleName { get; set; } = null!;

    public string Permissions { get; set; } = null!;
  
    public virtual ICollection<Emp> Emps { get; set; } = new List<Emp>();

}
