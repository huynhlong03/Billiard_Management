using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Thucdon
    {
        public Thucdon()
        {
            Chitiethoadons = new HashSet<Chitiethoadon>();
        }

        public string Mathucdon { get; set; } = null!;
        public string Tenthucdon { get; set; } = null!;
        public string Donvitinh { get; set; } = null!;
        public decimal Gia { get; set; }
        public string? Hinh { get; set; }
        public string? Ghichu { get; set; }

        public virtual ICollection<Chitiethoadon> Chitiethoadons { get; set; }
    }
}
