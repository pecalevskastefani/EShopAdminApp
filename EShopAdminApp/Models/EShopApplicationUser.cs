using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EShopAdminApp.Models
{
    public class EShopApplicationUser
    {
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string Username { get; set; }
        public string NormalizedUsername { get; set; }
    }
}
