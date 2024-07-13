namespace Portal.Models.ViewModels
{
    public class EditTransferMoneyVM
    {
        public long SpendMoneyId { get; internal set; }
        public long ReceivePaymentId { get; internal set; }
        public double Amount { get; internal set; }
        public string Date { get; internal set; }
        public string Description { get; internal set; }
        public long FromNameId { get; internal set; }
        public long ToNameId { get; internal set; }
    }
}
