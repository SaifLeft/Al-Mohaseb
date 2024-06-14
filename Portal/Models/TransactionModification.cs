using System.Globalization;
using System.Text.RegularExpressions;

namespace Portal.Models
{
    public class TransactionModification
    {
        public double OriginalAmount { get; set; }
        public DateTime OriginalTransactionDate { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Reason { get; set; }
        public double NewAmount { get; set; }
        public DateTime ModificationDate { get; set; }
    }
}
