using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API_Commandes.Models;
using Serilog;
using RabbitMQ.Client;
using API_Commandes.Messaging;

namespace API_Commandes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IStockCheckPublisher _stockCheckPublisher;
        private readonly IStockCheckResponseConsumer _stockCheckResponseConsumer;
        private readonly ICustomerCheckPublisher _customerCheckPublisher;
        private readonly ICustomerCheckResponseConsumer _customerCheckResponseConsumer;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(AppDbContext context,
                              IStockCheckPublisher stockCheckPublisher,
                              IStockCheckResponseConsumer stockCheckResponseConsumer,
                              ICustomerCheckPublisher customerCheckPublisher,
                              ICustomerCheckResponseConsumer customerCheckResponseConsumer, // Ajout� ici
                              ILogger<OrdersController> logger)
        {
            _context = context;
            _stockCheckPublisher = stockCheckPublisher;
            _stockCheckResponseConsumer = stockCheckResponseConsumer;
            _customerCheckPublisher = customerCheckPublisher;
            _customerCheckResponseConsumer = customerCheckResponseConsumer; // Initialisation
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder([FromBody] Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Publier un message via RabbitMQ pour v�rifier le client
                _customerCheckPublisher.PublishCustomerCheckRequest(order.CustomerId);

                // Attendre la r�ponse avec un d�lai
                var clientResponse = await _customerCheckResponseConsumer.WaitForCustomerCheckResponseAsync();

                if (clientResponse != null && clientResponse.IsCustomerValid)
                {
                    // Publier un message via RabbitMQ pour v�rifier le stock
                    _stockCheckPublisher.PublishStockCheckRequest(order);

                    // Attendre la r�ponse avec un d�lai
                    var stockResponse = await _stockCheckResponseConsumer.WaitForStockCheckResponseAsync();

                    if (stockResponse != null && stockResponse.IsStockAvailable)
                    {
                        order.Status = "Validated";
                        order.Date = DateTime.Now;
                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        var newOrder = await _context.Orders
                            .Include(o => o.OrderItems)
                            .Include(o => o.Payments)
                            .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                        return Ok(newOrder);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { Message = $"Order {order.OrderId} is rejected due to insufficient stock." });
                    }
                }
                else
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { Message = $"Order {order.OrderId} is rejected due to unverified client." });
                }
            }
            catch (TimeoutException ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(504, new { Message = "Stock check request timed out.", Error = ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Message = "An error occurred while processing the order.", Error = ex.Message });
            }
        }



        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _context.Orders
                 .Include(o => o.OrderItems)
                 .Include(o => o.Payments)
                 .ToListAsync();

            if (orders == null)
            {
                return NotFound();
            }

            return orders;
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        public async Task<ActionResult<Order>> PutOrder(int id, Order order)
        {
            if (id != order.OrderId)
            {
                return BadRequest();
            }

            var existingOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (existingOrder == null)
            {
                return NotFound();
            }

            // Mise � jour des propri�t�s
            _context.Entry(existingOrder).CurrentValues.SetValues(order);

            // Mise � jour des OrderItems
            existingOrder.OrderItems = order.OrderItems;

            // Mise � jour des Payments
            existingOrder.Payments = order.Payments;

            try
            {
                await _context.SaveChangesAsync();

                // Recharger la commande avec ses relations
                var updatedOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Include(o => o.Payments)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                return Ok(updatedOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
