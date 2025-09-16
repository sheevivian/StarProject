using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class CompanyNotify
{
    public int No { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string Category { get; set; } = null!;

    public DateTime PublishDate { get; set; }
}
