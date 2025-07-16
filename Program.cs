using Microsoft.EntityFrameworkCore;
using NaimaBeauty.Data;
using NaimaBeauty.Models;
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


builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

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


var app = builder.Build();

// // Enable Swagger in development
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers(); // Map API controllers

app.Run();
