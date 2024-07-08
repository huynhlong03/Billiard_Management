using System;
using System.Collections.Generic;

namespace Billiard_Management.Models
{
    public partial class Hoadon
    {
        public Hoadon()
        {
            Chitiethoadons = new HashSet<Chitiethoadon>();
        }

        public decimal Mahoadon { get; set; }
        public string Maban { get; set; } = null!;
        public DateTime? Giobatdau { get; set; }
        public DateTime? Gioketthuc { get; set; }
        public decimal? Thoigianchoi { get; set; }
        public decimal? Thanhtoan { get; set; }
        public decimal Khachhang { get; set; }
        public decimal? Tongtien { get; set; }
        public string? Ghichu { get; set; }
        public string? Trangthaithanhtoan { get; set; }
        public string Taikhoan { get; set; } = null!;

        public virtual Khachhang KhachhangNavigation { get; set; } = null!;
        public virtual Ban MabanNavigation { get; set; } = null!;
        public virtual Account TaikhoanNavigation { get; set; } = null!;
        public virtual ICollection<Chitiethoadon> Chitiethoadons { get; set; }
    }
}
