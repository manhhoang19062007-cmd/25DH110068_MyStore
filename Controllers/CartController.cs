using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH110068_MyStore.Models;

namespace _25DH110068_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Lấy giỏ hàng từ Session
        public List<CartItem> GetCart()
        {
            List<CartItem> cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // 1. Hiển thị trang giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        // 2. Thêm sản phẩm vào giỏ hàng
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                var cart = GetCart();
                var cartItem = cart.FirstOrDefault(p => p.Product.ProductID == id);
                if (cartItem == null)
                {
                    cart.Add(new CartItem { Product = product, Quantity = quantity });
                }
                else
                {
                    cartItem.Quantity += quantity;
                }
            }
            return RedirectToAction("Index");
        }

        // 3. Cập nhật số lượng
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(p => p.Product.ProductID == id);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }
            }
            return RedirectToAction("Index");
        }

        // 4. Xóa một sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(p => p.Product.ProductID == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }
            return RedirectToAction("Index");
        }

        // 5. Xóa toàn bộ giỏ hàng
        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}