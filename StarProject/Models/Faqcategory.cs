using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class Faqcategory
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Faq> Faqs { get; set; } = new List<Faq>();
}
