using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Faq
{
    public int No { get; set; }

    public int CategoryNo { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public DateTime UpdateDate { get; set; }

    public virtual Faqcategory CategoryNoNavigation { get; set; } = null!;

    public virtual ICollection<Faqkeyword> Faqkeywords { get; set; } = new List<Faqkeyword>();
}
