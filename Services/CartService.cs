using NaimaBeauty.Models;
using NaimaBeauty.Data;
using NaimaBeauty.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaimaBeauty.Services
{
    public class CartService : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cart>> GetAllAsync()
        {
            return await _context.Carts.ToListAsync();
        }

        public async Task<Cart?> GetByIdAsync(int id)
        {
            return await _context.Carts.FindAsync(id);
        }

        public async Task AddAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, Cart cart)
        {
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
  
             public async Task DeleteAsync(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart != null)
            {
                // First remove all related cart items
                _context.CartItems.RemoveRange(cart.CartItems);

                // Then remove the cart
                _context.Carts.Remove(cart);

                await _context.SaveChangesAsync();
            }
        }


    }
}
