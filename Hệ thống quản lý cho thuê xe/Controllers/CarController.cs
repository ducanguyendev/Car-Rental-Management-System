using System.Linq;
using System.Net;
using System.Web.Mvc;
using Hệ_thống_quản_lý_cho_thuê_xe.Models;

namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class CarController : Controller
    {
        private CarRentalDBEntities db = new CarRentalDBEntities(); // DbContext sinh ra từ EDMX

        // GET: Car
        public ActionResult Index()
        {
            return View(db.Cars.ToList());
        }

        // GET: Car/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Car/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car)
        {
            if (ModelState.IsValid)
            {
                db.Cars.Add(car);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(car);
        }

        // GET: Car/Edit/5
        public ActionResult Edit(int id)
        {
            Car car = db.Cars.Find(id);
            if (car == null) return HttpNotFound();
            return View(car);
        }

        // POST: Car/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Car car)
        {
            if (ModelState.IsValid)
            {
                db.Entry(car).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(car);
        }

        // GET: Car/Delete/5
        public ActionResult Delete(int id)
        {
            Car car = db.Cars.Find(id);
            if (car == null) return HttpNotFound();
            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Car car = db.Cars.Find(id);
            db.Cars.Remove(car);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Car/Details/5
        public ActionResult Details(int id)
        {
            Car car = db.Cars.Find(id);
            if (car == null) return HttpNotFound();
            return View(car);
        }
        // Thêm chức năng cho thuê xe
        public ActionResult Rent(string brand)
        {
            // Lấy danh sách hãng xe duy nhất
            var brands = db.Cars.Select(c => c.Brand).Distinct().ToList();
            ViewBag.Brands = new SelectList(brands);

            // Lọc theo hãng (nếu có)
            var cars = db.Cars.AsQueryable();
            if (!string.IsNullOrEmpty(brand) && brand != "All")
            {
                cars = cars.Where(c => c.Brand == brand);
            }

            return View(cars.ToList());
        }

    }
}
