using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
using System.Text;
using Microsoft.AspNetCore.Identity;
using NaimaBeauty.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NaimaBeauty.Services;
using NaimaBeauty.Interfaces;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register DbContext with Azure SQL connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Configure CORS to allow the React app 






builder.Services.AddIdentity<Customer, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

 //Register the roles controller
builder.Services.AddScoped<RolesController>();

//Services - business logic layer
builder.Services.AddScoped<ICartItemRepository, CartItemService>();
builder.Services.AddScoped<ICartRepository, CartService>();
builder.Services.AddScoped<ICategoryRepository, CategoryService>();
builder.Services.AddScoped<ICustomerRepository, CustomerService>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemService>();
builder.Services.AddScoped<IOrderRepository, OrderService>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryService>();
builder.Services.AddScoped<IProductRepository, ProductService>();


// Configure JWT token validation parameters (Issuer, Audience, Signing Key, etc.).
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


//Swagger configuration for API documentation 







builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();

// app.UseCors("AllowAll");
// app.UseStaticFiles(); 
// app.MapGet("/test-image", () => Results.Ok("Static files are working!"));

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger(); // Enables the Swagger JSON generation
//     app.UseSwaggerUI(options => 
//     {
//         options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
//         options.RoutePrefix = string.Empty; 
//     });

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();
app.MapControllers(); // Map API controllers
app.Logger.LogInformation("Application starting up...");
app.Run();
