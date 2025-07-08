using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NaimaBeauty.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
