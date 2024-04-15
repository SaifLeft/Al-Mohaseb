namespace Portal.Models
{
    public class ReasonsTable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }
        public string Date { get; internal set; }
    }
}
