﻿using System.ComponentModel.DataAnnotations;

namespace Backend
{
    public class UserDto // user data transfer object
    {
     //   [Key]
       
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
