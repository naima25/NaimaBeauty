using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
using NaimaBeauty.Interfaces;

namespace NaimaBeauty.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        // // Retrieves all carts
        // public async Task<IEnumerable<Cart>> GetAllAsync()
        // {
        //     return await _context.Carts.ToListAsync();
        // }

        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.Customer)
                .AsNoTracking()
                .ToListAsync();
        }

            public async Task<Cart?> GetByIdAsync(int id)
            {
                return await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .Include(c => c.Customer)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);
            }


        // // Retrieves a cart by its ID
        // public async Task<Cart?> GetByIdAsync(int id)
        // {
        //     return await _context.Carts.FindAsync(id);
        // }

        // Adds a new cart
        public async Task AddAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Updates an existing cart
        public async Task UpdateAsync(int id, Cart cart)
        {
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        //     // Deletes a cart by ID
        //     public async Task DeleteAsync(int id)
        //     {
        //         var cart = await _context.Carts.FindAsync(id);
        //         if (cart != null)
        //         {
        //             _context.Carts.Remove(cart);
        //             await _context.SaveChangesAsync();
        //         }
        //     }
        // }

        public async Task DeleteAsync(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart != null)
            {
                // Explicitly remove the cart items
                _context.CartItems.RemoveRange(cart.CartItems);

                // Then remove the cart
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();
            }
        }

    }
}
