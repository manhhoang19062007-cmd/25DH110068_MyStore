using _25DH110068_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110068_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tên tài khoản đã tồn tại chưa
                var checkUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
                if (checkUser != null)
                {
                    ModelState.AddModelError("", "Tên đăng nhập này đã được sử dụng!");
                    return View(model);
                }

                // Lưu vào bảng User
                var newUser = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu dạng text theo CSDL bài học
                    UserRole = "C" // C: Customer
                };
                db.Users.Add(newUser);

                // Lưu vào bảng Customer
                var newCustomer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerPhone = model.CustomerPhone,
                    CustomerEmail = model.CustomerEmail,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(newCustomer);

                db.SaveChanges();

                // Đăng nhập tự động sau khi đăng ký thành công
                Session["User"] = newUser;
                Session["Customer"] = newCustomer;

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
        public ActionResult Login()
        {
            return View();
        }

        // 2. Đăng nhập (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    Session["User"] = user;
                    var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
                    Session["Customer"] = customer;

                    // Nếu là Admin thì chuyển vào trang quản trị
                    if (user.UserRole == "A")
                    {
                        return RedirectToAction("Index", "Products", new { area = "Admin" });
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // 3. Đăng xuất
        public ActionResult Logout()
        {
            Session["User"] = null;
            Session["Customer"] = null;
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult ProfileInfo()
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = (User)Session["User"];
            var customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
            return View(customer);
        }

        // 4. POST: Cập nhật thông tin cá nhân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileInfo(Customer model)
        {
            if (Session["User"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var customer = db.Customers.Find(model.CustomerID);
                if (customer != null)
                {
                    customer.CustomerName = model.CustomerName;
                    customer.CustomerPhone = model.CustomerPhone;
                    customer.CustomerEmail = model.CustomerEmail;
                    customer.CustomerAddress = model.CustomerAddress;

                    db.SaveChanges();
                    Session["Customer"] = customer;
                    ViewBag.Message = "Cập nhật thông tin cá nhân thành công!";
                }
            }
            return View(model);
        }
    }
}



