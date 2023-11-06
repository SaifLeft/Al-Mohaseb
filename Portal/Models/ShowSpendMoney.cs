namespace Portal.Models
{
    public class ShowSpendMoney
    {
        public long PersonId { get; internal set; }
        public string PersonName { get; internal set; }
        public double? Amount { get; internal set; }
        public long? SpendMoneyId { get; internal set; }
    }
}
