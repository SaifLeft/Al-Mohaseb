using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Portal.Models.ViewModels
{
    public class ReceivePaymentsVM
    {
        [Required(ErrorMessage = "السبب مطلوب")]
        public long NameId { get; set; }

        [Required(ErrorMessage = "التاريخ مطلوب")]
        public string Date { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "السبب مطلوب")]
        public List<long> ReasonsList { get; set; }
    }
}
