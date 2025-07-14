using System.Collections.Generic;
using System.Threading.Tasks;
using NaimaBeauty.Models;

namespace NaimaBeauty.Repositories
{
    public interface IProductCategoryRepository
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync();
        Task AddAsync(ProductCategory productCategory);
        Task DeleteAsync(int productId, int categoryId);
    }
}
