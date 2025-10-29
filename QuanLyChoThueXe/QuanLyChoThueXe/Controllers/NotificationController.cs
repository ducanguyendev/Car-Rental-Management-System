using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyChoThueXe.Data;
using QuanLyChoThueXe.Models;

namespace QuanLyChoThueXe.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notification
        public async Task<IActionResult> Index(int? customerId, NotificationStatus? statusFilter)
        {
            var notificationsQuery = _context.Notifications
                .Include(n => n.Customer)
                .Include(n => n.Car)
                .AsQueryable();

            if (customerId.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.CustomerId == customerId.Value);
            }

            if (statusFilter.HasValue)
            {
                notificationsQuery = notificationsQuery.Where(n => n.Status == statusFilter.Value);
            }

            ViewBag.CustomerId = customerId;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.NotificationStatuses = Enum.GetValues<NotificationStatus>();
            ViewBag.Customers = await _context.Customers.ToListAsync();

            return View(await notificationsQuery.OrderByDescending(n => n.CreatedAt).ToListAsync());
        }

        // GET: Notification/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Customer)
                .Include(n => n.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            // Đánh dấu là đã đọc
            if (notification.Status == NotificationStatus.Unread)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.Now;
                _context.Update(notification);
                await _context.SaveChangesAsync();
            }

            return View(notification);
        }

        // GET: Notification/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            ViewBag.NotificationTypes = Enum.GetValues<NotificationType>();
            return View();
        }

        // POST: Notification/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CarId,Title,Content,Type")] Notification notification)
        {
            if (ModelState.IsValid)
            {
                notification.CreatedAt = DateTime.Now;
                _context.Add(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo thông báo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            ViewBag.NotificationTypes = Enum.GetValues<NotificationType>();
            return View(notification);
        }

        // GET: Notification/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            ViewBag.NotificationTypes = Enum.GetValues<NotificationType>();
            return View(notification);
        }

        // POST: Notification/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,CarId,Title,Content,Type,Status,CreatedAt")] Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật thông báo thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = await _context.Customers.Where(c => c.Status == CustomerStatus.Active).ToListAsync();
            ViewBag.Cars = await _context.Cars.ToListAsync();
            ViewBag.NotificationTypes = Enum.GetValues<NotificationType>();
            return View(notification);
        }

        // GET: Notification/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .Include(n => n.Customer)
                .Include(n => n.Car)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notification/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa thông báo thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Notification/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.Now;
                _context.Update(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đánh dấu đã đọc thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Notification/MarkAsUnread/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsUnread(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.Status = NotificationStatus.Unread;
                notification.ReadAt = null;
                _context.Update(notification);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đánh dấu chưa đọc thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Send notification when car becomes available
        [HttpPost]
        public async Task<IActionResult> NotifyCarAvailable(int carId)
        {
            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe" });
            }

            // Tìm các booking đang chờ xe này
            var pendingBookings = await _context.Bookings
                .Where(b => b.CarId == carId && b.Status == BookingStatus.Pending)
                .Include(b => b.Customer)
                .ToListAsync();

            var notificationsCreated = 0;

            foreach (var booking in pendingBookings)
            {
                var notification = new Notification
                {
                    CustomerId = booking.CustomerId,
                    CarId = carId,
                    Title = $"Xe {car.Name} đã có sẵn",
                    Content = $"Xe {car.Name} (biển số: {car.LicensePlate}) mà bạn đã đặt trước hiện đã có sẵn. Vui lòng liên hệ để xác nhận thuê xe.",
                    Type = NotificationType.CarAvailable,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                notificationsCreated++;
            }

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Đã gửi thông báo cho {notificationsCreated} khách hàng",
                notificationsCreated = notificationsCreated
            });
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}
