using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class EditSpendMoneyVM
    {
        [Required(ErrorMessage = "الرقم مطلوب")]
        public long SpendMoneyId { get; set; }
        [Required(ErrorMessage = "السبب مطلوب")]
        public long NameId { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public string Date { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        public double Amount { get; set; }
    }
}
