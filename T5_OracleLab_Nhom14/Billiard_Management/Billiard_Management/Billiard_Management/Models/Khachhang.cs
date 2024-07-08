using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Khachhang
    {
        public Khachhang()
        {
          
            Hoadons = new HashSet<Hoadon>();
        }

        public decimal Makh { get; set; }
        public string Ten { get; set; } = null!;
        public string? Phone { get; set; }
        public decimal? Loaithanhvien { get; set; }

        public virtual Loaithanhvien? LoaithanhvienNavigation { get; set; }
       
        public virtual ICollection<Hoadon> Hoadons { get; set; }
    }
}
