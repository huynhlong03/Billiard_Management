using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Chitiethoadon
    {
        public decimal Machitiethoadon { get; set; }
        public decimal Mahoadon { get; set; }
        public string Mathucdon { get; set; } = null!;
        public decimal Soluongdat { get; set; }

        public virtual Hoadon MahoadonNavigation { get; set; } = null!;
        public virtual Thucdon MathucdonNavigation { get; set; } = null!;
    }
}
