using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Models;
using NaimaBeauty.Data;
using System.Linq;
using NaimaBeauty.Interfaces;

namespace NaimaBeauty.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly AppDbContext _context;

        public ProductCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync() =>
            await _context.ProductCategories.ToListAsync();

        public async Task AddAsync(ProductCategory productCategory)
        {
            _context.ProductCategories.Add(productCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int productId, int categoryId)
        {
            var entity = await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);

            if (entity != null)
            {
                _context.ProductCategories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
