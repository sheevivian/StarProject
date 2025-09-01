using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Knowledge
{
    public int No { get; set; }

    public string Category { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Source { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public int? Like { get; set; }
}
