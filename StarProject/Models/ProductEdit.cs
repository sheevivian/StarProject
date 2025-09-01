using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class ProductEdit
{
    public string EmpNo { get; set; } = null!;

    public int ProductNo { get; set; }

    public string Motion { get; set; } = null!;

    public DateTime Update { get; set; }

    public virtual Emp EmpNoNavigation { get; set; } = null!;

    public virtual Product ProductNoNavigation { get; set; } = null!;
}
