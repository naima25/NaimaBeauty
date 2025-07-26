using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Models;
using NaimaBeauty.Data;
using NaimaBeauty.Interfaces;

namespace NaimaBeauty.Repositories
{
    public class CustomerRepository
    {
        private readonly AppDbContext _context;

        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync() =>
        await _context.Customers
            .Include(c => c.Orders)
            .ThenInclude(o => o.OrderItems)
            .ToListAsync();

        public async Task<Customer?> GetByIdAsync(string id) =>
            await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(string id, Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(string id)
        {
            var customer = await _context.Customers
                 .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
    }
}

    //      public async Task DeleteAsync(string id)
    //      {
    //          var customer = await _context.Customers.FindAsync(id);
    //          if (customer != null)
    //          {
    //              _context.Customers.Remove(customer);
    //              await _context.SaveChangesAsync();
    //          }
    //      }
    // }

