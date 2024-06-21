
namespace Portal.Models
{
    public class GeneralBalanceVM
    {
        public double AllReceivePaymentsAmount { get; internal set; }
        public double AllSpendMoneyAmount { get; internal set; }
        public double GeneralBalance { get; internal set; }
        public List<SelectListModel> YearsList { get; internal set; }
        public double Zakat { get; internal set; }
    }
}
