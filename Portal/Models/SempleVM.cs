namespace Portal.Models
{
    public class SempleVM
    {
        public double AllReceivePaymentsAmount { get; internal set; }
        public double AllSpendMoneyAmount { get; internal set; }
        public double GeneralBalance { get; internal set; }
        public bool PersonalBalanceIsAvailable { get; internal set; }
        public double PersonalReceivePayment { get; internal set; }
        public double PersonalSpendMoney { get; internal set; }
        public double PersonalTotalAmount { get; internal set; }
        public List<SelectListModel> NamesList { get; internal set; }
        public List<SelectListModel> YearsList { get; internal set; }
        public int SelectedYear { get; internal set; }
        public bool YearIsAvailable { get; internal set; }
        public double SpendMoneyYearlyData { get; internal set; }
        public double ReceivePaymentsYearlyData { get; internal set; }
        public double TotalAmountYearlyData { get; internal set; }
        public List<ShowPersonsTotal> PersonsBalance { get; internal set; }
    }
}
