# Naimabeauty Backend API

## Project Overview

Naimabeauty Backend API is a **.NET 9.0 Web API** designed to manage the backend operations for the Naimabeauty application. It follows **RESTful principles** and includes features such as:

- User authentication and authorization with JWT  
- Database management with Entity Framework Core and SQL Server   
- API documentation with Swagger (Swashbuckle)  
- Logging and monitoring with Microsoft.Extensions.Logging  

This backend API powers Naimabeauty’s frontend by providing endpoints for users, orders, products, and other beauty-related data.  

---

## Key Technologies

- **.NET 9.0** (ASP.NET Core Web API)  
- **Entity Framework Core** with SQL Server for data access  
- **ASP.NET Core Identity** for user and role management  
- **JWT Bearer Authentication** for secure API access   
- **Swashbuckle (Swagger)** for API documentation  
- **Microsoft.Extensions.Logging** for logging  

---

## Features

- RESTful API endpoints for managing users, products, and orders  
- JWT Authentication & Authorization  
- Database interactions with Entity Framework Core  
- PDF generation for reports and invoices  
- Swagger UI for API testing and documentation  
- Logging and monitoring for better reliability  

---

## Requirements

To run this project locally, you need:

- **.NET 9.0 SDK**  
- **SQL Server** (or SQL Server Express)  
- Postman or a similar API testing tool  

---

## Installation & Running the Project

1. **Clone the repository**  

```bash
git clone https://github.com/naima25/Naimabeauty-Backend.git
cd Naimabeauty-Backend

2. **Restore packages**

dotnet restore

3. Run the application

dotnet run





