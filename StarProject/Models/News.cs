using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class News
{
    public int No { get; set; }

    public string Category { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime PublishDate { get; set; }
}
