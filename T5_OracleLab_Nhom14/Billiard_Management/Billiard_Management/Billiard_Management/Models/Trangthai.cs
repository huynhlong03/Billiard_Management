using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Trangthai
    {
        public Trangthai()
        {
            Bans = new HashSet<Ban>();
        }

        public decimal Matrangthai { get; set; }
        public string? Tentrangthai { get; set; }

        public virtual ICollection<Ban> Bans { get; set; }
    }
}
