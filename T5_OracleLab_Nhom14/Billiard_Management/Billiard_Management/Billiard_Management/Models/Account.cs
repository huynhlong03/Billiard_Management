namespace Billiard_Management.Models
{
    public class Account
    {
        public Account()
        {
            Hoadons = new HashSet<Hoadon>();
        }
        public string TaiKhoan { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string HoTen { get; set; }
        public string SDT { get; set; } = null!;
        public string TinhTrang { get; set; }
        public int? QuanLy { get; set; }
        public virtual ICollection<Hoadon> Hoadons { get; set; }
    }
}
