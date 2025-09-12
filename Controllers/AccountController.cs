using NaimaBeauty.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


// AccountController handles user registration, login and JWT token generation
// It allows users to register, log in, log out


namespace NaimaBeauty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Customer> _userManager; //UserManager is used to handle operations related to user accounts
        private readonly SignInManager<Customer> _signInManager; //SignInManager is responsible for handling the authentication of users
    
        private readonly IConfiguration _configuration;  // Access configuration settings, like JWT key

        // Constructor to inject dependencies
        public AccountController(UserManager<Customer> userManager, SignInManager<Customer> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

    
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest model)
        {
            if (model == null)
                return BadRequest("Request body is null");

            Console.WriteLine($"Email: {model.Email}, Password: {model.Password}");

            var user = new Customer { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);
                return Ok(new { Token = token });
            }

            // Return detailed errors from Identity if registration fails
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            //Attempt to sign in the user using the provided email and password
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email); // If successful, find the user by email using UserManager
                var roles = await _userManager.GetRolesAsync(user);   // Get the roles of the user (used for role-based authorisation)
                var token = GenerateJwtToken(user,roles);  // Generate a JWT (JSON Web Token) for the logged-in user
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid login attempt.");
        }

        //Logs out the currently authenticated user
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out");
        }
        
        // Generates a JWT token for the user, including their roles
        private string GenerateJwtToken(Customer user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            // add user id as a claim
            claims.Add(new Claim("userId", user.Id.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
