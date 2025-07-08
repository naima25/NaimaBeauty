using System.Collections.Generic;
using System.Text.Json.Serialization;
 
namespace NaimaBeauty.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
 
        // Many-to-many relationship with Product through ProductCategory
        [JsonIgnore]
        public List<ProductCategory>? ProductCategories { get; set; }
    }
}