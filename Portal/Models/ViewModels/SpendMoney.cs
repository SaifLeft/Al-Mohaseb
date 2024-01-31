using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Models.ViewModels
{
    public class SpendMoneyVM
    {
        public List<SelectListItem> NamesList { get; internal set; }
        public List<SelectListItem> YearList { get; internal set; }
    }
}
