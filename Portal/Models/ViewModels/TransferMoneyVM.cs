namespace Portal.Models.ViewModels
{
    public class TransferMoneyVM
    {
        public long FromNameId { get; set; }
        public long ToNameId { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string description { get; set; }
    }
}
