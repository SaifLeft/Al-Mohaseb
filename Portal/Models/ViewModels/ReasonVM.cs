using Portal.Data;
using System.ComponentModel.DataAnnotations;

namespace Portal.Models.ViewModels
{
    public class ReasonVM
    {
        public List<ReasonsTable> Reasons { get; set; }
        public bool FromQuery { get; internal set; }
        public MosbReasons FromQearyDetails { get; internal set; }
        public int ReasonId { get; internal set; }
        public bool? AddStatus { get; internal set; }
        public bool? UpdateStatus { get; internal set; }
        public bool? DeleteStatus { get; internal set; }
    }
}
