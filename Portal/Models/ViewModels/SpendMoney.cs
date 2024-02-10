using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Models.ViewModels
{
    public class SpendMoneyVM
    {
        public List<SelectListItem> NamesList { get; internal set; }
        public List<SelectListItem> YearList { get; internal set; }
        public bool? AddStatus { get; set; }
        public bool? UpdateStatus { get; set; }
    }
}
