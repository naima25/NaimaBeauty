using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using NaimaBeauty.Models; 
using System.Text.Json.Serialization;


namespace NaimaBeauty.Models
{
    //Inherits from IdentityUser which adds authentication and identity features
    public class Customer : IdentityUser
    {
        public string? FullName { get; set; }  

      
        // [JsonIgnore]
        public List<Order>? Orders { get; set; }    // One-to-many relationship

        // Navigation property to ProductCustomer table (Many-to-Many with Product)
        public List<ProductCustomer>? ProductCustomers { get; set; }  // Tracks products purchased by this customer
    }
}
