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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(AppDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                 .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(c => c.Customer)
                .ToListAsync();
        }

        // GET: api/Cart/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cart>> GetCart(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                 .ThenInclude(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }


        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult<Cart>> CreateCart(Cart cart)
        {
            // Validate products exist
            if (cart.CartItems != null && cart.CartItems.Any())
            {
                var productIds = cart.CartItems.Select(ci => ci.ProductId).Distinct();
                var existingProducts = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();

                var missingProducts = productIds.Except(existingProducts).ToList();
                if (missingProducts.Any())
                {
                    return BadRequest($"Products not found: {string.Join(",", missingProducts)}");
                }
            }

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
        }

        // PUT: api/Cart/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, CartDto cartDto)
        {
            if (id != cartDto.Id)
            {
                return BadRequest();
            }

            // Get existing cart with items from DB
            var existingCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCart == null)
            {
                return NotFound();
            }

            // Update cart properties from DTO
            existingCart.CustomerId = cartDto.CustomerId;
            existingCart.Price = cartDto.Price;

            if (cartDto.CartItems != null)
            {
                // Remove items not in updated DTO list
                var itemsToRemove = existingCart.CartItems
                    .Where(existing => !cartDto.CartItems.Any(updated => updated.ProductId == existing.ProductId))
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _context.CartItems.Remove(item);
                }

                // Update existing or add new cart items
                foreach (var updatedItem in cartDto.CartItems)
                {
                    var existingItem = existingCart.CartItems
                        .FirstOrDefault(ci => ci.ProductId == updatedItem.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity = updatedItem.Quantity;
                    }
                    else
                    {
                        existingCart.CartItems.Add(new CartItem
                        {
                            ProductId = updatedItem.ProductId,
                            Quantity = updatedItem.Quantity
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/Cart/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
            {
                return NotFound();
            }

            // Remove cart items first
            _context.CartItems.RemoveRange(cart.CartItems);

            // Then remove the cart
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
