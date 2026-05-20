using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StorageManager.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class CreateUserRequest
    {
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(256)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public List<string> Roles { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(256)]
        public string Email { get; set; }

        // optional: if provided, password will be updated
        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        // null => leave roles unchanged, empty list => clear roles, otherwise replace
        public List<string> Roles { get; set; }
    }
}