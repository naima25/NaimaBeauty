using System.Collections.Generic;
using System.Threading.Tasks;
using NaimaBeauty.Models;

namespace NaimaBeauty.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(int id);
        Task AddAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(int id);
    }
}
