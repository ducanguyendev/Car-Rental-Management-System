using System.Linq;
using System.Web.Mvc;
using Hệ_thống_quản_lý_cho_thuê_xe.Models; // đổi đúng namespace Models

namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class AccountController : Controller
    {
        private CarRentalDBEntities db = new CarRentalDBEntities();

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Tạm thời so sánh trực tiếp (plaintext). 
            // Sau này thay bằng hash để bảo mật.
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                Session["UserId"] = user.UserId;
                Session["Username"] = user.Username;
                Session["Role"] = user.Role;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
            return View();
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public ActionResult Register(string username, string password, string fullname, string email)
        {
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            if (!email.EndsWith("@email.com"))
            {
                ViewBag.Error = "Email phải có dạng ...@email.com!";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                PasswordHash = password, // ⚠ tạm lưu plaintext, sau nên hash
                FullName = fullname,
                Role = "User",
                Email = email
            };

            db.Users.Add(newUser);
            db.SaveChanges();

            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại trong hệ thống.";
                return View();
            }

            // Nếu tìm thấy, chuyển đến form đổi mật khẩu
            TempData["EmailToReset"] = email;
            return RedirectToAction("ResetPassword");
        }
        [HttpGet]
        public ActionResult ResetPassword()
        {
            if (TempData["EmailToReset"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = TempData["EmailToReset"];
            TempData.Keep("EmailToReset");
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string newPassword, string confirmPassword)
        {
            string email = TempData["EmailToReset"] as string;
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                TempData.Keep("EmailToReset");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp.";
                TempData.Keep("EmailToReset");
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy người dùng.";
                return RedirectToAction("ForgotPassword");
            }

            // ⚠ Nếu có mã hóa mật khẩu, bạn cần hash mật khẩu tại đây
            user.PasswordHash = newPassword;
            db.SaveChanges();

            TempData["Message"] = "Đặt lại mật khẩu thành công. Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

    }
}
