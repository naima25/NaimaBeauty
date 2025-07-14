using NaimaBeauty.Models;
using NaimaBeauty.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaimaBeauty.Interfaces;

namespace NaimaBeauty.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves all Products from the database asynchronously
        public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();

        // Retrieves a Product by ID from the database asynchronously
        public async Task<Product> GetByIdAsync(int id) => await _context.Products.FindAsync(id);

        // Adds a Product to the database asynchronously
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        // Updates a Product in the database asynchronously
        public async Task UpdateAsync(Product product)
        {
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Deletes a Product by ID from the database asynchronously
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
