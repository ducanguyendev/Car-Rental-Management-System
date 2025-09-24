using System.Web;
using System.Web.Mvc;

namespace Hệ_thống_quản_lý_cho_thuê_xe
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
