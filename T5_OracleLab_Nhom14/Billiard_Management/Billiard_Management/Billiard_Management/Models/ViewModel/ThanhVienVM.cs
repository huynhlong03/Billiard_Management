using Microsoft.AspNetCore.Mvc.Rendering;

namespace Billiard_Management.Models.ViewModel
{
    public class ThanhVienVM
    {
        public decimal Makh { get; set; }
        public string Ten { get; set; }
        public string Phone { get; set; }
        public decimal? SelectedLoaiThanhVienId { get; set; }   
       // public List<SelectListItem> LoaithanhvienOptions { get; set; }
    }
}
