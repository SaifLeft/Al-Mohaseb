namespace Portal.Models.ViewModels
{
    public class StatisticsVM
    {
        public int NamesCount { get; set; }
        public double ReceivePaymentsAmountCount { get; set; }
        public double SpendMoneyAmountCount { get; set; }
        public List<string> MonthInEn { get; internal set; }
        public List<BerYear> ReceivePaymentsBerYear { get; internal set; }
        public List<BerYear> SpendMoneyBerYear { get; internal set; }
        public List<int> FromFirstYearToLastYearInReceivePayments { get; internal set; }
        public List<int> FromFirstYearToLastYearInSpendMoney { get; internal set; }
        public bool YearSelected { get; internal set; }
        public int? FirstYear { get; internal set; }
        public int? SecondYear { get; internal set; }
    }
}
