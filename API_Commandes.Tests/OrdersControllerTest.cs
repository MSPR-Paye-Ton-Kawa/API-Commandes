using API_Commandes.Controllers;
using API_Commandes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API_Commandes.Tests
{
    public class OrdersControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public OrdersControllerTests()
        {
            // Configuration du contexte de base de données en mémoire pour les tests
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using (var context = new AppDbContext(_dbContextOptions))
            {
                context.Orders.AddRange(
                    new Order
                    {
                        OrderId = 1,
                        CustomerId = 1,
                        Date = DateTime.Now.AddDays(-10),
                        Status = "Completed",
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem { OrderItemId = 1, ProductId = 1, Quantity = 2 },
                            new OrderItem { OrderItemId = 2, ProductId = 2, Quantity = 3 }
                        },
                        Payments = new List<Payment>
                        {
                            new Payment { PaymentId = 1, Amount = 50.00m, PaymentDate = DateTime.Now.AddDays(-5), PaymentMethod = "Credit Card", Status = "Completed" }
                        }
                    },
                    new Order
                    {
                        OrderId = 2,
                        CustomerId = 2,
                        Date = DateTime.Now.AddDays(-5),
                        Status = "Pending",
                        OrderItems = new List<OrderItem>
                        {
                            new OrderItem { OrderItemId = 3, ProductId = 3, Quantity = 1 }
                        },
                        Payments = new List<Payment>
                        {
                            new Payment { PaymentId = 2, Amount = 20.00m, PaymentDate = DateTime.Now.AddDays(-2), PaymentMethod = "PayPal", Status = "Pending" }
                        }
                    }
                );

                context.SaveChanges();
            }
        }

        [Fact]
        public async Task GetOrders_ReturnsAllOrders()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);

                // Act
                var result = await controller.GetOrders();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Order>>>(result);
                var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(actionResult.Value);
                Assert.Equal(2, orders.Count());
            }
        }

        [Fact]
        public async Task GetOrder_WithExistingId_ReturnsOrder()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);
                int orderId = 1;

                // Act
                var result = await controller.GetOrder(orderId);

                // Assert
                var actionResult = Assert.IsType<ActionResult<Order>>(result);
                var order = Assert.IsAssignableFrom<Order>(actionResult.Value);
                Assert.Equal(orderId, order.OrderId);
            }
        }

        [Fact]
        public async Task GetOrder_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);
                int nonExistingId = 999;

                // Act
                var result = await controller.GetOrder(nonExistingId);

                // Assert
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task PutOrder_ExistingId_UpdatesOrder()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);
                int orderId = 1;
                var updatedOrder = new Order
                {
                    OrderId = orderId,
                    CustomerId = 1,
                    Date = DateTime.Now,
                    Status = "Shipped"
                };

                // Act
                var result = await controller.PutOrder(orderId, updatedOrder);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var order = context.Orders.Find(orderId);
                Assert.Equal("Shipped", order.Status);
            }
        }

        [Fact]
        public async Task PostOrder_CreatesOrder()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);
                var newOrder = new Order
                {
                    CustomerId = 3,
                    Date = DateTime.Now,
                    Status = "Pending"
                };

                // Act
                var result = await controller.PostOrder(newOrder);

                // Assert
                var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var order = Assert.IsType<Order>(createdAtActionResult.Value);
                Assert.Equal(3, order.CustomerId);
            }
        }

        [Fact]
        public async Task DeleteOrder_ExistingId_DeletesOrder()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);
                int orderId = 1;

                // Act
                var result = await controller.DeleteOrder(orderId);

                // Assert
                Assert.IsType<NoContentResult>(result);
                var order = context.Orders.Find(orderId);
                Assert.Null(order);
            }
        }

        [Fact]
        public async Task GetOrdersWithPayments_ReturnsOrdersWithPayments()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);

                // Act
                var result = await controller.GetOrdersWithPayments();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Order>>>(result);
                var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(actionResult.Value);
                Assert.All(orders, o => Assert.NotNull(o.Payments));
            }
        }

        [Fact]
        public async Task GetOrdersWithOrderItems_ReturnsOrdersWithOrderItems()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);

                // Act
                var result = await controller.GetOrdersWithOrderItems();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Order>>>(result);
                var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(actionResult.Value);
                Assert.All(orders, o => Assert.NotNull(o.OrderItems));
            }
        }

        [Fact]
        public async Task GetOrdersWithAll_ReturnsOrdersWithAllDetails()
        {
            // Arrange
            using (var context = new AppDbContext(_dbContextOptions))
            {
                var controller = new OrdersController(context);

                // Act
                var result = await controller.GetOrdersWithAll();

                // Assert
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Order>>>(result);
                var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(actionResult.Value);
                Assert.All(orders, o =>
                {
                    Assert.NotNull(o.Payments);
                    Assert.NotNull(o.OrderItems);
                });
            }
        }

    }
}
