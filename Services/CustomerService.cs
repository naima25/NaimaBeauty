using NaimaBeauty.Models;
using NaimaBeauty.Data;
using NaimaBeauty.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace NaimaBeauty.Services
{
    public class CustomerService : ICustomerRepository
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Customer> _userManager;

        public CustomerService(AppDbContext context, UserManager<Customer> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

     public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .ToListAsync();
        }

public async Task<Customer?> GetByIdAsync(string id)
{
    return await _context.Customers
        .Include(c => c.Orders)
        .ThenInclude(o => o.OrderItems)
        .FirstOrDefaultAsync(c => c.Id == id);
}

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
            var customer = await _userManager.FindByIdAsync(id);
            if (customer != null)
            {
                var result = await _userManager.DeleteAsync(customer);
                if (!result.Succeeded)
                {
                    // You can handle errors here, throw or log
                    throw new System.Exception("Failed to delete user: " + string.Join(", ", result.Errors));
                }
            }
        }
    }
}
