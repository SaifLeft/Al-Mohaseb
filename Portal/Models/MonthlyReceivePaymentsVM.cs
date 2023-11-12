namespace Portal.Models
{
    public class MonthlyReceivePaymentsVM
    {
        public bool IsSelected { get; set; } = false;
        public string YearMonth { get;  set; }
        public bool IsHasRecodes { get;  set; }
        public List<ShowMonthlyReceivePayments> SumitedMonthlyPayments { get; set; } = new List<ShowMonthlyReceivePayments>();
    }
    public class MonthlyReceiveRow
    {
        public long PersonId { get; set; }
        public double Amount { get; set; }
        public bool IsPaid { get; set; }
    }
    public class MonthlyReceiveRecord
    {
        public List<MonthlyReceiveRow> MonthlyPaidData { get; set; }
        public string YearMonth { get; set; }
    }
}
