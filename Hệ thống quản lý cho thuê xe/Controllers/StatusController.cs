using System.Linq;
using System.Web.Mvc;
using Hệ_thống_quản_lý_cho_thuê_xe.Models;

namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class StatusController : Controller
    {
        private CarRentalDBEntities db = new CarRentalDBEntities();

        // GET: Status
        public ActionResult Index(string searchString)
        {
            var cars = from c in db.Cars
                       select c;

            // Nếu có nhập tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                cars = cars.Where(c => c.LicensePlate.Contains(searchString)
                                    || c.Brand.Contains(searchString)
                                    || c.Model.Contains(searchString));
            }

            return View(cars.ToList());
        }
    }
}
