using System.ComponentModel.DataAnnotations;

namespace Billiard_Management.Models.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Nhập username!")]
        public string TaiKhoan { get; set; }
        [DataType(DataType.Password), Required(ErrorMessage = "Nhập Password!")]
        public string MatKhau { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
