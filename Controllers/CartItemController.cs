using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For async database methods
using Microsoft.Extensions.Logging; // For ILogger

namespace NaimaBeauty.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartItemController : ControllerBase
    {
        private readonly AppDbContext _context; // DbContext for SQLite
        private readonly ILogger<CartItemController> _logger;  // Injecting the logger

        public CartItemController(AppDbContext context, ILogger<CartItemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/CartItem
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItem>>> Get()
        {
            try
            {
                _logger.LogInformation("Fetching all cart items with products.");
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)  // Explicitly load the Product
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    _logger.LogWarning("No cart items found.");
                    return NotFound("No cart items found.");
                }

                _logger.LogInformation($"Retrieved {cartItems.Count} cart items with products.");
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch cart items.");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/CartItem/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CartItem>> Get(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching cart item {id} with product.");
                var cartItem = await _context.CartItems
                    .Include(c => c.Product)  // Explicitly load the Product
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cartItem == null)
                {
                    _logger.LogWarning($"Cart item {id} not found.");
                    return NotFound($"Cart item {id} not found.");
                }

                _logger.LogInformation($"Successfully retrieved cart item {id}.");
                return Ok(cartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch cart item {id}.");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<CartItem>> Create([FromBody] CartItem newCartItem)
        {
            try
            {
                // 1. Validate input
                if (newCartItem == null)
                {
                    _logger.LogWarning("Received empty cart item object.");
                    return BadRequest("Cart item data cannot be null.");
                }

                _logger.LogInformation("Creating cart item for ProductId: {ProductId}, Quantity: {Quantity}", 
                    newCartItem.ProductId, newCartItem.Quantity);

                // 2. Check if product exists (more efficient than FirstOrDefault)
                var productExists = await _context.Products
                    .AsNoTracking()
                    .AnyAsync(p => p.Id == newCartItem.ProductId);

                if (!productExists)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", newCartItem.ProductId);
                    return NotFound($"Product with ID {newCartItem.ProductId} not found.");
                }

                // 3. Clear navigation property to prevent duplicate insert
                newCartItem.Product = null;

                // 4. Save the cart item
                _context.CartItems.Add(newCartItem);
                await _context.SaveChangesAsync();

                // 5. Fetch the COMPLETE cart item with product (cleaner than manual attach)
                var createdCartItem = await _context.CartItems
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.Id == newCartItem.Id);

                _logger.LogInformation("Successfully created cart item with ID: {CartItemId}", createdCartItem.Id);
                return CreatedAtAction(nameof(Get), new { id = createdCartItem.Id }, createdCartItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cart item for ProductId: {ProductId}", 
                    newCartItem?.ProductId ?? 0);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        // PUT: api/CartItem/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<CartItem>> Update(int id, [FromBody] CartItem updatedCartItem)
        {
            try
            {
                _logger.LogInformation($"Updating cart item with ID {id}.");

                var existingCartItem = await _context.CartItems.FindAsync(id);
                if (existingCartItem == null)
                {
                    _logger.LogWarning($"Cart item with ID {id} not found for update.");
                    return NotFound($"Cart item with ID {id} not found.");
                }

                _logger.LogInformation($"Found CartItem with ID {id}, updating properties.");

                // Validate if the ProductId exists
                _logger.LogInformation("Checking if Product with ID {ProductId} exists.", updatedCartItem.ProductId);
                var productExists = await _context.Products.AnyAsync(p => p.Id == updatedCartItem.ProductId);
                if (!productExists)
                {
                    _logger.LogWarning($"Product with ID {updatedCartItem.ProductId} does not exist.");
                    return BadRequest($"Product with ID {updatedCartItem.ProductId} does not exist.");
                }

                // Update fields
                existingCartItem.ProductId = updatedCartItem.ProductId;
                existingCartItem.Quantity = updatedCartItem.Quantity;

                _logger.LogInformation("Saving updated CartItem with ID {id}.");
                await _context.SaveChangesAsync();

                // Reload the CartItem including its Product
                var updatedWithProduct = await _context.CartItems
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                _logger.LogInformation($"CartItem with ID {id} updated successfully.");
                return Ok(updatedWithProduct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating cart item.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Concurrency issue while updating the cart item.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the cart item.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        // DELETE: api/CartItem/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting cart item with ID {id}.");
                var cartItemToRemove = await _context.CartItems.FindAsync(id);
                if (cartItemToRemove == null)
                {
                    _logger.LogWarning($"CartItem with ID {id} not found for deletion.");
                    return NotFound($"CartItem with ID {id} not found.");
                }

                _logger.LogInformation($"Removing CartItem with ID {id} from the database.");
                _context.CartItems.Remove(cartItemToRemove);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"CartItem with ID {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the cart item.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
