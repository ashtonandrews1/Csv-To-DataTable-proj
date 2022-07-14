using System.ComponentModel;

namespace PortalProject.Models
{
    public class MarketItem
    {
        [DisplayName("Date")]
        public DateTime? Date { get; set; }

        [DisplayName("Price")]
        public string? Price { get; set; }
    }
}