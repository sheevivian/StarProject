using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Collection
{
    public string UserNo { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int? ProductNo { get; set; }

    public int? EventNo { get; set; }

    public int? KnowledgeNo { get; set; }

    public string Image { get; set; } = null!;

    public DateTime Date { get; set; }

    public virtual Event? EventNoNavigation { get; set; }

    public virtual Knowledge? KnowledgeNoNavigation { get; set; }

    public virtual Product? ProductNoNavigation { get; set; }

    public virtual User UserNoNavigation { get; set; } = null!;
}
