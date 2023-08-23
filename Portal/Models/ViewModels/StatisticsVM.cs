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
        public bool SMYearSelected { get; internal set; }
        public int? RPFirstYear { get; internal set; }
        public int? RPSecondYear { get; internal set; }
        public int NamesCountInCertainYear { get; internal set; }
        public int? AddCountYear { get; internal set; }
        public int ReasonsCountInCertainYear { get; internal set; }
        public (string ResoneName, double Amount) ReceivePaymentsFromCertainYear { get; internal set; }
        public int? SMFirstYear { get; internal set; }
        public int? SMSecondYear { get; internal set; }
        public bool RPYearSelected { get; internal set; }
    }
}
