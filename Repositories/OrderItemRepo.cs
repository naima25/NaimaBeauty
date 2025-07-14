using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Models;
using NaimaBeauty.Data;
using NaimaBeauty.Interfaces;

namespace NaimaBeauty.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;

        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync() =>
            await _context.OrderItems.ToListAsync();

        public async Task<OrderItem?> GetByIdAsync(int id) =>
            await _context.OrderItems.FindAsync(id);

        public async Task AddAsync(OrderItem orderItem)
        {
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            _context.Entry(orderItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
