using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EShopAdminApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public EShopApplicationUser OrderBy { get; set; }
        public List<ProductInOrder> Products { get; set; }
    }
}
