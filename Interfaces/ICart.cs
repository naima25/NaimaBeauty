using System.Collections.Generic;
using System.Threading.Tasks;
using NaimaBeauty.Models;

namespace NaimaBeauty.Interfaces
{
    public interface ICartRepository
    {
        Task<IEnumerable<Cart>> GetAllAsync();
        Task<Cart?> GetByIdAsync(int id);
        Task AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(int id);
    }
}
