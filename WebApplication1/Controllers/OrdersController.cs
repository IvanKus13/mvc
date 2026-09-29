using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var orders = _context.Orders
                .Include(o => o.Car)
                .Where(o => o.UserId == userId)
                .ToList();

            return View(orders);
        }

        public IActionResult Buy(int carId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var order = new Order
            {
                CarId = carId,
                UserId = userId.Value,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Автомобіль успішно придбано!";

            return RedirectToAction("Index", "Orders");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            ViewBag.Cars = await _context.Cars.ToListAsync();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Cars = await _context.Cars.ToListAsync();
                return View(model);
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            order.OrderDate = model.OrderDate;
            order.CarId = model.CarId;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Замовлення успішно змінене!";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Car)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound();

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Замовлення успішно видалено!";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delivery(int orderId)
        {
            var alreadyDelivered = _context.DeliveryInfos
                .Any(d => d.OrderId == orderId);

            if (alreadyDelivered)
            {
                TempData["ErrorMessage"] =
                    "Для цього замовлення доставка вже оформлена!";

                return RedirectToAction("Index");
            }

            var model = new DeliveryInfo
            {
                OrderId = orderId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delivery(DeliveryInfo model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("MODEL INVALID");

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(model);
            }

            _context.DeliveryInfos.Add(model);
            _context.SaveChanges();

            TempData["SuccessMessage"] =
                "Доставка успішно оформлена! Очікуйте повідомлення.";

            return RedirectToAction("Index");
        }
    }

    public class OrderDateValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value,
            ValidationContext validationContext)
        {
            DateTime date = (DateTime)value;

            DateTime minDate = new DateTime(2000, 1, 1);
            DateTime maxDate = DateTime.Now;

            if (date < minDate)
            {
                return new ValidationResult(
                    "Дата замовлення не може бути старішою за 2000 рік.");
            }

            if (date > maxDate)
            {
                return new ValidationResult(
                    "Дата замовлення не може бути з майбутнього.");
            }

            return ValidationResult.Success;
        }
    }
}