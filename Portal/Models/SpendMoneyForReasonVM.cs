using Portal.Data;

namespace Portal.Models
{
    public class SpendMoneyForReasonVM
    {
        public List<ShowSpendMoney> PersonConnectedWithReasonId { get; set; }
        public bool IsSelected { get;  set; } = false;
        public long ReasonId { get;  set; }
        public double AmountSubscribed { get;  set; }
        public List<ShowSpendMoney> SpendMoneySubmitedAmount { get; set; }
        public string YearMonth { get; internal set; }
        public bool IsHasRecod { get; internal set; }
    }
}
