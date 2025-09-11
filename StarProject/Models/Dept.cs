using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Dept
{
    public int No { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public string? DeptDescription { get; set; }

    public bool IsActive { get; set; }
}
