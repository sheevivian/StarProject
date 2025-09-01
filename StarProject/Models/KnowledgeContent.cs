using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class KnowledgeContent
{
    public int KnowledgeNo { get; set; }

    public int OrderNo { get; set; }

    public string BlockType { get; set; } = null!;

    public string? Content { get; set; }

    public string? Image { get; set; }

    public virtual Knowledge KnowledgeNoNavigation { get; set; } = null!;
}
