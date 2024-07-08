namespace Billiard_Management.Models
{
    public class Ban
    {
        public Ban()
        {
            //Dattruocs = new HashSet<Dattruoc>();
            Hoadons = new HashSet<Hoadon>();
        }
        public string Maban { get; set; } = null!;
        public string? Tenban { get; set; }
        public string? Loaiban { get; set; }
        public decimal? Trangthai { get; set; }
        public decimal? Gia { get; set; }
        public string? Hinhanh { get; set; }

        public virtual LoaiBan? LoaibanNavigation { get; set; }
        public virtual Trangthai? TrangthaiNavigation { get; set; }
        public virtual ICollection<Hoadon> Hoadons { get; set; }
    }
}
