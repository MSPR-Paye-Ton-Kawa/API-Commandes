namespace API_Commandes.Models
{
        public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }  // Managed externally
        public int Quantity { get; set; }
    }
   
}
