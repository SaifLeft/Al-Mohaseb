namespace Portal.Models
{
    public class SpendMoneyForReasonDTO
    {
        public long PersonId { get; set; }
        public long ReasonsId { get; set; }
        public string Date { get; set; }
        public double Amount { get; set; }
        public bool IsPaid { get; set; }
    }
}
