
namespace Portal.Models.ViewModels
{
    public class PersonalBalanceVM
    {
        public List<SelectListModel> YearsList { get; internal set; }
        public List<SelectListModel> NamesList { get; internal set; }
        public bool PersonalBalanceIsAvailable { get; internal set; }
        public double PersonalReceivePayment { get; internal set; }
        public double PersonalSpendMoney { get; internal set; }
        public double PersonalTotalAmount { get; internal set; }
        public double Zakat { get; internal set; }
        public string? Name { get; internal set; }
        public int? Year { get; internal set; }
    }
}
