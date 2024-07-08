using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Loaithanhvien
    {
        public Loaithanhvien()
        {
            Khachhangs = new HashSet<Khachhang>();
        }

        public decimal Maloaitv { get; set; }
        public string? Tenloaitv { get; set; }
        public decimal? Giamgia { get; set; }
        public string? Ghichu { get; set; }

        public virtual ICollection<Khachhang> Khachhangs { get; set; }
    }
}
