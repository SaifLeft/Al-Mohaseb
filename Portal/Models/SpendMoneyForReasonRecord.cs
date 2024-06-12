namespace Portal.Models
{
    public class SpendMoneyForReasonRecord
    {
        public long ReasonId { get; set; }
        public bool IsRecord { get; set; }
        public string MonthYear { get; set; }
        public double MonthlyAmountRecord { get; set; }
        public List<PersonIdAmount> PersonIdAmount { get; set; }
    }
    public class PersonIdAmount
    {
        public long PersonId { get; set; }
        public double Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsFixed { get; set; }
    }
}
