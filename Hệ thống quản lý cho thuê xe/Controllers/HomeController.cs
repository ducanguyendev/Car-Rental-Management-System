using Hệ_thống_quản_lý_cho_thuê_xe.Models;
using System.Web.Mvc;
using System.Linq;

public class HomeController : Controller
{
    private CarRentalDBEntities db = new CarRentalDBEntities();

    public ActionResult Index()
    {
        var cars = db.Cars.ToList();
        return View(cars); // Truyền danh sách xe qua View
    }
}
