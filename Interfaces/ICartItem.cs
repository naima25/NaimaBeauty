using NaimaBeauty.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaimaBeauty.Interfaces
{
    public interface ICartItemRepository
    {
        Task<IEnumerable<CartItem>> GetAllAsync();
        Task<CartItem?> GetByIdAsync(int id);
        Task AddAsync(CartItem cartItem);
        Task UpdateAsync(CartItem cartItem);
        Task DeleteAsync(int id);
    }
}
