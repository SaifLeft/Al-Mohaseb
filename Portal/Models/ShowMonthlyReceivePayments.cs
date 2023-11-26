namespace Portal.Models
{
    public class ShowMonthlyReceivePayments
    {
        public double Amount { get;  set; }
        public long MonthlyReceivePaymentsId { get;  set; }
        public string PersonName { get;  set; }
        public long PersonId { get;  set; }
        public bool IsPaid { get;  set; }
        public double SubscriptionAmount { get; internal set; }
    }
}
