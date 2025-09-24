using System.Linq;
using System.Net;
using System.Web.Mvc;
using Hệ_thống_quản_lý_cho_thuê_xe.Models; // namespace EDMX của bạn

namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class CustomerController : Controller
    {
        private CarRentalDBEntities db = new CarRentalDBEntities(); // Tên DbContext từ EDMX

        // GET: Customer
        public ActionResult Index()
        {
            // Include Car để lấy luôn thông tin xe
            var customers = db.Customers.Include("Car").ToList();
            return View(customers);
        }

        // GET: Customer/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Include("Car")
                                            .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // GET: Customer/Create
        public ActionResult Create()
        {
            ViewBag.CarId = new SelectList(db.Cars, "CarId", "CarName");
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerId,FullName,IdentityCard,Phone,CarId")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CarId = new SelectList(db.Cars, "CarId", "Brand", customer.CarId);
            return View(customer);
        }


        // GET: Customer/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Find(id);
            if (customer == null)
                return HttpNotFound();

            ViewBag.CarId = new SelectList(db.Cars, "CarId", "CarName", customer.CarId);
            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Trả lại view cùng dữ liệu nếu ModelState invalid
            return View(customer);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Customer customer = db.Customers.Include("Car")
                                            .FirstOrDefault(c => c.CustomerId == id);

            if (customer == null)
                return HttpNotFound();

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var customer = db.Customers.Find(id);

            if (customer != null)
            {
                // Xóa tất cả Reservation liên quan đến Customer này
                var reservations = db.Reservations.Where(r => r.CustomerId == id).ToList();
                foreach (var res in reservations)
                {
                    db.Reservations.Remove(res);
                }

                db.Customers.Remove(customer);
                db.SaveChanges();
            }

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
