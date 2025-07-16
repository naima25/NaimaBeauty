using System.Collections.Generic;
using System.Threading.Tasks;
using NaimaBeauty.Data;
using NaimaBeauty.Interfaces;
using NaimaBeauty.Models;
using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Repositories;

namespace NaimaBeauty.Services
{
    public class ProductCategoryService : IProductCategoryRepository
    {
        private readonly AppDbContext _context;

        public ProductCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return await _context
                .ProductCategories.Include(pc => pc.Product) // Fetch product details (including the name)
                .Include(pc => pc.Category) // Fetch category details (including the name)
                .ToListAsync();
        }

        public async Task AddAsync(ProductCategory pc)
        {
            _context.ProductCategories.Add(pc);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int productId, int categoryId)
        {
            var pc = await _context.ProductCategories.FindAsync(productId, categoryId);
            if (pc != null)
            {
                _context.ProductCategories.Remove(pc);
                await _context.SaveChangesAsync();
            }
        }
    }
}
