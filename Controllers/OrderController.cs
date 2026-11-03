using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using _25DH110068_MyStore.Models;

namespace _25DH110068_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // 1. GET: Xác nhận thông tin đặt hàng (Checkout)
        public ActionResult Checkout()
        {
            // Kiểm tra xem khách đã đăng nhập chưa
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra giỏ hàng có trống không
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = (User)Session["User"];
            var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);

            ViewBag.Cart = cart;
            ViewBag.TotalAmount = cart.Sum(i => i.TotalPrice);

            return View(customer);
        }

        // 2. POST: Lưu đơn hàng vào CSDL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string shippingAddress)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = (User)Session["User"];
            var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);

            // 1. Tạo bản ghi đơn hàng mới (Order)
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                AddressDelivery = string.IsNullOrEmpty(shippingAddress) ? customer.CustomerAddress : shippingAddress,
                TotalAmount = cart.Sum(i => i.TotalPrice)
            };
            db.Orders.Add(order);
            db.SaveChanges(); // Lưu để sinh OrderID tự động

            // 2. Lưu từng món trong giỏ vào chi tiết đơn hàng (OrderDetail)
            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.Product.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.ProductPrice
                };
                db.OrderDetails.Add(orderDetail);
            }
            db.SaveChanges();

            // 3. Xóa giỏ hàng sau khi đặt thành công
            Session["Cart"] = null;

            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // 3. GET: Trang thông báo đặt hàng thành công
        public ActionResult OrderSuccess(int id)
        {
            ViewBag.OrderID = id;
            return View();
        }

        // 4. GET: Xem lịch sử đơn hàng của tôi
        public ActionResult MyOrder()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = (User)Session["User"];
            var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID)
                                  .OrderByDescending(o => o.OrderDate)
                                  .ToList();

            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult OrderDetails(int id)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var orderDetails = db.OrderDetails.Where(d => d.OrderID == id).ToList();
            ViewBag.Order = order;
            return View(orderDetails);
        }
        [HttpPost]
        public ActionResult PaymentWithPaypal()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Tỉ giá quy đổi VND -> USD (giả định 1 USD = 24,000 VND để gửi sang Paypal)
            decimal exchangeRate = 24000;
            decimal totalVND = cart.Sum(i => i.TotalPrice);
            decimal totalUSD = Math.Round(totalVND / exchangeRate, 2);

            // Cấu hình thông tin gửi sang Sandbox PayPal
            string businessEmail = "sb-seller@business.example.com"; // Email tài khoản người bán sandbox
            string returnUrl = Url.Action("PaymentSuccess", "Order", null, Request.Url.Scheme);
            string cancelUrl = Url.Action("Checkout", "Order", null, Request.Url.Scheme);

            string redirectUrl = $"https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_xclick" +
                                 $"&business={businessEmail}" +
                                 $"&item_name=Don_hang_MyStore" +
                                 $"&amount={totalUSD}" +
                                 $"&currency_code=USD" +
                                 $"&return={returnUrl}" +
                                 $"&cancel_return={cancelUrl}";

            return Redirect(redirectUrl);
        }

        // GET: Xử lý sau khi hoàn tất thanh toán từ PayPal quay về
        public ActionResult PaymentSuccess()
        {
            var cart = Session["Cart"] as List<CartItem>;
            var user = Session["User"] as User;

            if (cart != null && user != null)
            {
                var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);

                // Lưu đơn hàng đã thanh toán vào CSDL
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    AddressDelivery = customer.CustomerAddress + " (Đã thanh toán PayPal)",
                    TotalAmount = cart.Sum(i => i.TotalPrice)
                };
                db.Orders.Add(order);
                db.SaveChanges();

                foreach (var item in cart)
                {
                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.Product.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.ProductPrice
                    };
                    db.OrderDetails.Add(detail);
                }
                db.SaveChanges();

                // Xóa giỏ hàng
                Session["Cart"] = null;

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}