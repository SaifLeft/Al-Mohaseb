using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class PersonVM
    {
        [Required(ErrorMessage = "السبب مطلوب")]
        public string Name { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public int Phone { get; set; }
        [Required(ErrorMessage = "السبب مطلوب")]
        public int ReasonId { get; set; }
        [Required(ErrorMessage = "الرقم المدني مطلوب")]
        public int CivilNumber { get; set; }
    }
}
