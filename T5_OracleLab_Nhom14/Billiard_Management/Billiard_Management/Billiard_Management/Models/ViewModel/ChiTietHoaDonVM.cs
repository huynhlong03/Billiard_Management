namespace Billiard_Management.Models.ViewModel
{
    public class ChiTietHoaDonVM
    {
        public Hoadon HoaDon { get; set; }
        public IEnumerable<Chitiethoadon> Chitiethoadon { get; set; }
        public Chitiethoadon newChiTietHoaDon { get; set; }
        public Ban Ban { get; set; }
        public Khachhang KhachHang { get; set; }
        public IEnumerable<Thucdon> ThucDon { get; set; }
    }

}
