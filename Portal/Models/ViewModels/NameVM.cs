using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class NameVM
    {
        [Required(ErrorMessage = "السبب مطلوب")]
        public string Name { get; set; }
        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        public long Phone { get; set; }
        [Required(ErrorMessage = "الرقم المدني مطلوب")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "الرقم المدني يجب ان يكون ارقام فقط")]
        [Display(Name = "الرقم المدني")]

        public long CivilNumber { get; set; }
        
        public double SubscriptionAmount { get; set; }
    }
}
