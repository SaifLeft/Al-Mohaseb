using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class EditReceivePaymentsVM
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "السبب مطلوب")]
        public long NameId { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public string Date { get; set; }
        [Required(ErrorMessage = "السبب مطلوب")]

        public string Description { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        public double Amount { get; set; }
    }
}
