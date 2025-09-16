using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace StarProject.Models;

public partial class Schedule
{
    public int EventNo { get; set; }

    public DateTime ReleaseDate { get; set; }

    public DateTime ExpirationDate { get; set; }

    public bool? Executed { get; set; }

    public virtual Event EventNoNavigation { get; set; } = null!;
}
