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
        public async Task<IActionResult> UpdateCart(int id, Cart cart)
        {
            if (id != cart.Id)
            {
                return BadRequest();
            }

            // Get existing cart with items
            var existingCart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCart == null)
            {
                return NotFound();
            }

            // Update cart properties
            existingCart.CustomerId = cart.CustomerId;
            existingCart.Price = cart.Price;

            // Handle cart items
            if (cart.CartItems != null)
            {
                // Remove items not in the updated cart
                var itemsToRemove = existingCart.CartItems
                    .Where(existing => !cart.CartItems.Any(updated => 
                        updated.ProductId == existing.ProductId))
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _context.CartItems.Remove(item);
                }

                // Update existing items or add new ones
                foreach (var updatedItem in cart.CartItems)
                {
                    var existingItem = existingCart.CartItems
                        .FirstOrDefault(ci => ci.ProductId == updatedItem.ProductId);

                    if (existingItem != null)
                    {
                        // Update quantity
                        existingItem.Quantity = updatedItem.Quantity;
                    }
                    else
                    {
                        // Add new item (without CartId since it's not in model)
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

// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Logging;
// using NaimaBeauty.Models;
// using NaimaBeauty.Services;

// namespace NaimaBeauty.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class CartController : ControllerBase
//     {
//         private readonly CartService _cartService;
//         private readonly ILogger<CartController> _logger;

//         public CartController(CartService cartService, ILogger<CartController> logger)
//         {
//             _cartService = cartService;
//             _logger = logger;
//         }

//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
//         {
//             var carts = await _cartService.GetAllAsync();
//             return Ok(carts);
//         }

//         [HttpGet("{id}")]
//         public async Task<ActionResult<Cart>> GetCart(int id)
//         {
//             var cart = await _cartService.GetByIdAsync(id);
//             if (cart == null)
//                 return NotFound();

//             return Ok(cart);
//         }

//         [HttpPost]
//         public async Task<ActionResult<Cart>> CreateCart(Cart cart)
//         {
//             // NOTE: Product validation logic stays in controller if needed
//             if (cart.CartItems != null && cart.CartItems.Any())
//             {
//                 var productIds = cart.CartItems.Select(ci => ci.ProductId).Distinct();
//                 // Ideally this validation should move to the service/repo layer
//                 return BadRequest("Product validation should move to service.");
//             }

//             await _cartService.AddAsync(cart);
//             return CreatedAtAction(nameof(GetCart), new { id = cart.Id }, cart);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateCart(int id, Cart cart)
//         {
//             if (id != cart.Id)
//                 return BadRequest();

//             await _cartService.UpdateAsync(id, cart);
//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteCart(int id)
//         {
//             await _cartService.DeleteAsync(id);
//             return NoContent();
//         }
//     }
// }
