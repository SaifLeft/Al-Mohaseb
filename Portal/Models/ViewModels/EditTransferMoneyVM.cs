using Portal.Data;

namespace Portal.Models.ViewModels
{
    public class EditTransferMoneyVM
    {
        public long SpendMoneyId { get; set; }
        public long ReceivePaymentId { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string FromNameText { get; internal set; }
        public string ToNameText { get; internal set; }
    }
    public class TransferMoneyEditRequest
    {

        public long SpendMoneyId { get; set; }
        public long ReceivePaymentId { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
    }
}
