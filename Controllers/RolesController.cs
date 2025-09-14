using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NaimaBeauty.Models;
using Microsoft.AspNetCore.Authorization;

//The RolesController manages operations related to user roles such as creating, updating, deleting, and assigning roles
//It is protected by the Admin role, meaning only users with the Admin role can access these actions

namespace NaimaBeauty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<Customer> _userManager;

        // Constructor to inject RoleManager and UserManager dependencies
        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<Customer> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult GetRoles() // Get all roles from the database
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles); // Return the list of roles as the response
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            return Ok(role);
        }

        // Creates a new role with the specified role name
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Ok("Role created successfully.");
            }

            return BadRequest(result.Errors);
        }

        // Updates an existing role with a new name
        [HttpPut]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest model)
        {
            var role = await _roleManager.FindByIdAsync(model.RoleId);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            role.Name = model.NewRoleName;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return Ok("Role updated successfully.");
            }

            return BadRequest(result.Errors);
        }

        //Deletes a role by its ID

        [HttpDelete]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok("Role deleted successfully.");
            }

            return BadRequest(result.Errors);
        }

        //  Assigns a specific role to a user
        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] RoleAssignment model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);

            if (!roleExists)
            {
                return NotFound("Role not found.");
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);

            if (result.Succeeded)
            {
                return Ok("Role assigned to user successfully.");
            }

            return BadRequest(result.Errors);
        }

    }
}

