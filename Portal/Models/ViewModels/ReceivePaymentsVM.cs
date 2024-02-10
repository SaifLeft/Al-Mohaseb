using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Models.ViewModels
{
    public class ReceivePaymentsVM
    {
        public List<SelectListItem> Names { get; internal set; }
        public bool? AddStatus { get; internal set; }
        public bool? UpdateStatus { get; internal set; }
    }
}
