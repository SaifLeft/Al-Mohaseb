using Portal.Data;

namespace Portal.Models
{
    public class SpendMoneyForReasonVM
    {
        public List<ShowSpendMoney> AllPersonInSystem { get; set; }
        public bool IsSelected { get;  set; } = false;
        public long ReasonId { get;  set; }
        public double? AmountSubscribed { get;  set; }
        public List<ShowSpendMoney> SpendMoneySubmitedAmount { get; set; }
        public string YearMonth { get; internal set; }
        public bool IsHasRecodes { get; internal set; }
        public List<SelectListModel> Reasons { get; internal set; }
    }
}
