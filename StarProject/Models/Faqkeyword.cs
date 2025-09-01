using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Faqkeyword
{
    public int No { get; set; }

    public int FaqNo { get; set; }

    public string Keyword { get; set; } = null!;

    public virtual Faq FaqNoNavigation { get; set; } = null!;
}
