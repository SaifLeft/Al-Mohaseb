using Portal.Data;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ReasonVM
    {
        public List<(long,string,double)> Reasons { get; set; }
        public bool FromQuery { get; internal set; }
        public MosbReasons FromQearyDetails { get; internal set; }
        public int ReasonId { get; internal set; }
    }
}
