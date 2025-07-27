using System.ComponentModel.DataAnnotations;
using NaimaBeauty.Dtos;

namespace NaimaBeauty.Dtos
{
    public class OrderItemDto
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
