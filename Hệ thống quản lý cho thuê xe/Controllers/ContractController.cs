using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//Hợp đồng thuê xe
namespace Hệ_thống_quản_lý_cho_thuê_xe.Controllers
{
    public class ContractController : Controller
    {
        // GET: Contract
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            return View();
        }
    }
}