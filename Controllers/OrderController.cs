using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
using NaimaBeauty.Dtos;

namespace NaimaBeauty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            try
            {
                _logger.LogInformation("Fetching all orders with customers and items.");
                var orders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .ToListAsync();

                if (orders == null || !orders.Any())
                {
                    _logger.LogWarning("No orders found.");
                    return NotFound("No orders found.");
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching orders.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // GET: api/Order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching order {id} with customer and items.");
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {id} not found.");
                    return NotFound($"Order with ID {id} not found.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(OrderDto orderDto)
        {
            try
            {
                if (orderDto == null)
                {
                    _logger.LogWarning("Received empty order object.");
                    return BadRequest("Order data cannot be null.");
                }

                // Map OrderDto to Order entity
                var order = new Order
                {
                    CustomerId = orderDto.CustomerId,
                    Price = orderDto.Price,
                    OrderDate = orderDto.OrderDate,
                    OrderItems = orderDto.OrderItems?.Select(itemDto => new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity
                    }).ToList()
                };

                _logger.LogInformation("Creating new order for customer: {CustomerId}", order.CustomerId);

                // Verify customer exists
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == order.CustomerId);
                if (!customerExists)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found.", order.CustomerId);
                    return NotFound($"Customer with ID {order.CustomerId} not found.");
                }

                // Clear navigation properties to prevent duplicate inserts
                order.Customer = null;
                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Product = null;
                    }
                }

                // Set order date if not provided
                if (order.OrderDate == default)
                {
                    order.OrderDate = DateTime.UtcNow;
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Reload the complete order with relationships
                var createdOrder = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                _logger.LogInformation("Successfully created order with ID: {OrderId}", createdOrder.Id);
                return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

[HttpPut("{id}")]
public async Task<IActionResult> UpdateOrder(int id, OrderDto orderDto)
{
    try
    {
        // Map OrderDto to Order entity (to keep your existing logic unchanged)
        var order = new Order
        {
            Id = id, // Make sure the id matches the URL
            CustomerId = orderDto.CustomerId,
            Price = orderDto.Price,
            OrderDate = orderDto.OrderDate,
            OrderItems = orderDto.OrderItems?.Select(itemDto => new OrderItem
            {
                Id = itemDto.Id,        // assuming OrderItemDto has Id
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            }).ToList()
        };

        if (id != order.Id)
        {
            _logger.LogWarning("Order ID mismatch.");
            return BadRequest("Order ID mismatch.");
        }

        _logger.LogInformation($"Updating order with ID {id}");

        var existingOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (existingOrder == null)
        {
            _logger.LogWarning($"Order with ID {id} not found for update.");
            return NotFound($"Order with ID {id} not found.");
        }

        // Verify customer exists if changing customer
        if (existingOrder.CustomerId != order.CustomerId)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == order.CustomerId);
            if (!customerExists)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", order.CustomerId);
                return NotFound($"Customer with ID {order.CustomerId} not found.");
            }
        }

        // Update properties
        existingOrder.CustomerId = order.CustomerId;
        existingOrder.Price = order.Price;
        existingOrder.OrderDate = order.OrderDate;

        // Handle order items updates if needed
        // Update order item quantities
        foreach (var updatedItem in order.OrderItems)
        {
            var existingItem = existingOrder.OrderItems
                .FirstOrDefault(oi => oi.Id == updatedItem.Id);

            if (existingItem != null)
            {
                existingItem.Quantity = updatedItem.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        // Reload the complete order with relationships
        var updatedOrder = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        _logger.LogInformation($"Order with ID {id} updated successfully.");
        return Ok(updatedOrder);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogError(ex, "Concurrency error while updating order.");
        return StatusCode(StatusCodes.Status500InternalServerError, "Concurrency issue while updating the order.");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while updating the order.");
        return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
    }
}


       
        // DELETE: api/Order/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting order with ID {id}.");
                
                // Include related OrderItems in the query
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {id} not found.");
                    return NotFound($"Order with ID {id} not found.");
                }

                // Remove all order items first
                _context.OrderItems.RemoveRange(order.OrderItems);
                
                // Then remove the order
                _context.Orders.Remove(order);
                
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order with ID {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the order.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        }
    }
