using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Role
{
    public int No { get; set; }

    public string RoleName { get; set; } = null!;

    public bool Emp { get; set; }

    public bool User { get; set; }

    public bool Info { get; set; }

    public bool Event { get; set; }

    public bool Pd { get; set; }

    public bool Tic { get; set; }

    public bool Pm { get; set; }

    public bool Order { get; set; }

    public bool Cs { get; set; }

    public bool Oa { get; set; }

    public bool CoNlist { get; set; }

    public bool CoNe { get; set; }

    public virtual ICollection<Emp> Emps { get; set; } = new List<Emp>();
}
