
namespace Portal.Models
{
    public class DawnloadAllDataAsPDFVM
    {
        public List<ReasonsTable> Reasons { get; internal set; }
        public double AllReceivePaymentsAmount { get; internal set; }
        public double AllSpendMoneyAmount { get; internal set; }
        public double GeneralBalance { get; internal set; }
        public List<YearlyBalanceDTO> AllBalanceForEveryYear { get; internal set; }
        public List<ShowPersonsTotal> PersonsBalance { get; internal set; }
    }
}
