namespace Billiard_Management.Models
{
    public class LoaiBan
    {
        public LoaiBan()
        {
            Bans = new HashSet<Ban>();
        }
        public string MaLoai { get; set; } = null!;
        public string? TenLoai { get; set; }
        public virtual ICollection<Ban> Bans { get; set; }
    }
}
