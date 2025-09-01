using System;
using System.Collections.Generic;

namespace StarProject.Models;

public partial class StarMap
{
    public int No { get; set; }

    public string Name { get; set; } = null!;

    public string Desc { get; set; } = null!;

    public byte[] Image { get; set; } = null!;

    public decimal MapLatitude { get; set; }

    public decimal MapLongitude { get; set; }
}
