using System.Text.Json.Serialization;

namespace API_Commandes.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }  // Managed externally
        public DateTime Date { get; set; }
        public string Status { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }
}
