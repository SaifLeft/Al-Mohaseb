using Portal.Data;

namespace Portal.Models.ViewModels
{
    public class PersonVM
    {
        public bool FromQuery { get; internal set; }
        public MosbName? FromQearyDetails { get; internal set; }
        public int PersonId { get; internal set; }
    }
}
