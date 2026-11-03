using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110068_MyStore.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice
        {
            get
            {
                return Convert.ToDecimal(Product.ProductPrice) * Quantity;
            }
        }
    }
}