 public class UpdateRoleRequest
    {
       // Represents the data required to update the name of an existing role
       // This model is used when we need to modify the name of a role 
        public string ? RoleId { get; set; }
        public string ? NewRoleName { get; set; }
    }