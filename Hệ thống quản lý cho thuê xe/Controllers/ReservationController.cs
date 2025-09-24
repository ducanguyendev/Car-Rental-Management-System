using Hệ_thống_quản_lý_cho_thuê_xe.Models;
using System.Linq;
using System.Web.Mvc;
using System;

namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class ReservationController : Controller
    {
        private CarRentalDBEntities db = new CarRentalDBEntities();

        // Danh sách xe cho thuê (lọc theo hãng)
        public ActionResult Index(string brand)
        {
            var brands = db.Cars.Select(c => c.Brand).Distinct().ToList();
            ViewBag.Brands = new SelectList(brands);

            var cars = db.Cars.AsQueryable();
            if (!string.IsNullOrEmpty(brand) && brand != "All")
            {
                cars = cars.Where(c => c.Brand == brand);
            }

            return View(cars.ToList());
        }

        // GET: Reservation/Create (hiện form nhập thông tin khách hàng)
        public ActionResult Create(int carId)
        {
            var car = db.Cars.Find(carId);
            if (car == null) return HttpNotFound();

            ViewBag.Car = car;
            return View();
        }

        // POST: Reservation/Create (xử lý đặt xe)
        // POST: Reservation/Create (xử lý đặt xe)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int carId, string FullName, string Phone, string IdentityCard)
        {
            if (ModelState.IsValid)
            {
                // Tạo khách hàng mới, nhớ gán CarId
                var customer = new Customer
                {
                    FullName = FullName,
                    Phone = Phone,
                    IdentityCard = IdentityCard,
                    CarId = carId   // 👉 thêm dòng này
                };
                db.Customers.Add(customer);
                db.SaveChanges();

                // Tạo đơn đặt xe
                var reservation = new Reservation
                {
                    CarId = carId,
                    CustomerId = customer.CustomerId,
                    ReservationDate = DateTime.Now,
                    Status = "Pending"
                };
                db.Reservations.Add(reservation);

                // Cập nhật trạng thái xe
                var car = db.Cars.Find(carId);
                if (car != null)
                {
                    car.Status = "Rented";
                }

                db.SaveChanges();
                return RedirectToAction("Success");
            }

            // Nếu có lỗi thì load lại thông tin xe
            var reloadCar = db.Cars.Find(carId);
            ViewBag.Car = reloadCar;
            return View();
        }


        // Trang thông báo đặt xe thành công
        public ActionResult Success()
        {
            return View();
        }
    }
}
