using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NaimaBeauty.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrderItemController> _logger;

        public OrderItemController(AppDbContext context, ILogger<OrderItemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/OrderItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> Get()
        {
            try
            {
                _logger.LogInformation("Fetching all order items with products.");
                var orderItems = await _context.OrderItems
                    .Include(o => o.Product)  // Explicitly load the Product
                    .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .ToListAsync();

                if (orderItems == null || !orderItems.Any())
                {
                    _logger.LogWarning("No order items found.");
                    return NotFound("No order items found.");
                }

                _logger.LogInformation($"Retrieved {orderItems.Count} order items with products.");
                return Ok(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch order items.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/OrderItem/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> Get(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching order item {id} with product.");
                var orderItem = await _context.OrderItems
                    .Include(o => o.Product)  // Explicitly load the Product
                    .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (orderItem == null)
                {
                    _logger.LogWarning($"Order item {id} not found.");
                    return NotFound($"Order item {id} not found.");
                }

                _logger.LogInformation($"Successfully retrieved order item {id}.");
                return Ok(orderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch order item {id}.");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/OrderItem
        [HttpPost]
        public async Task<ActionResult<OrderItem>> CreateOrderItem([FromBody] OrderItem newOrderItem)
        {
            try
            {
                if (newOrderItem == null)
                {
                    _logger.LogWarning("Received empty order item object.");
                    return BadRequest("Order item data cannot be null.");
                }

                _logger.LogInformation("Creating order item for ProductId: {ProductId}, Quantity: {Quantity}", 
                    newOrderItem.ProductId, newOrderItem.Quantity);

                // Check if product exists
                var productExists = await _context.Products
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == newOrderItem.ProductId);

                if (!productExists)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", newOrderItem.ProductId);
                    return NotFound($"Product with ID {newOrderItem.ProductId} not found.");
                }

                // Clear navigation property to prevent duplicate insert
                newOrderItem.Product = null;

                _context.OrderItems.Add(newOrderItem);
                await _context.SaveChangesAsync();

                // Fetch the complete order item with product
                var createdOrderItem = await _context.OrderItems
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o => o.Id == newOrderItem.Id);

                _logger.LogInformation("Successfully created order item with ID: {OrderItemId}", createdOrderItem.Id);
                return CreatedAtAction(nameof(Get), new { id = createdOrderItem.Id }, createdOrderItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order item for ProductId: {ProductId}", 
                    newOrderItem?.ProductId ?? 0);
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/OrderItem/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderItem>> UpdateOrderItem(int id, [FromBody] OrderItem updatedOrderItem)
        {
            try
            {
                _logger.LogInformation($"Updating order item with ID {id}.");

                if (id != updatedOrderItem.Id)
                {
                    _logger.LogWarning("Order item ID mismatch.");
                    return BadRequest("Order item ID mismatch.");
                }

                var existingOrderItem = await _context.OrderItems.FindAsync(id);
                if (existingOrderItem == null)
                {
                    _logger.LogWarning($"Order item with ID {id} not found for update.");
                    return NotFound($"Order item with ID {id} not found.");
                }

                // Validate if the ProductId exists
                _logger.LogInformation("Checking if Product with ID {ProductId} exists.", updatedOrderItem.ProductId);
                var productExists = await _context.Products.AnyAsync(p => p.Id == updatedOrderItem.ProductId);
                if (!productExists)
                {
                    _logger.LogWarning($"Product with ID {updatedOrderItem.ProductId} does not exist.");
                    return BadRequest($"Product with ID {updatedOrderItem.ProductId} does not exist.");
                }

                // Update fields
                existingOrderItem.ProductId = updatedOrderItem.ProductId;
                existingOrderItem.Quantity = updatedOrderItem.Quantity;

                await _context.SaveChangesAsync();

                // Reload the OrderItem including its Product
                var updatedWithProduct = await _context.OrderItems
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                _logger.LogInformation($"Order item with ID {id} updated successfully.");
                return Ok(updatedWithProduct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating order item.");
                return StatusCode(500, "Concurrency issue while updating the order item.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the order item.");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/OrderItem/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderItem(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting order item with ID {id}.");
                var orderItemToRemove = await _context.OrderItems.FindAsync(id);
                if (orderItemToRemove == null)
                {
                    _logger.LogWarning($"Order item with ID {id} not found for deletion.");
                    return NotFound($"Order item with ID {id} not found.");
                }

                _context.OrderItems.Remove(orderItemToRemove);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Order item with ID {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the order item.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}