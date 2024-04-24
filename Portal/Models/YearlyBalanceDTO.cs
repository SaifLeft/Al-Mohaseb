namespace Portal.Models
{
    public class YearlyBalanceDTO
    {
        public string Year { get; internal set; }
        public double ReceivePayments { get; internal set; }
        public double SpendMoney { get; internal set; }
        public double TotalAmount { get; internal set; }
    }
}
