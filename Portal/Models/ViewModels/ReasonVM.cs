using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ReasonVM
    {
        public List<(long,string)> Reasons { get; set; }
    }
}
