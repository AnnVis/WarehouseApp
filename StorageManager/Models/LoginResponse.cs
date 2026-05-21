using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StorageManager.Models
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public int ExpiresIn { get; set; }
    }
}