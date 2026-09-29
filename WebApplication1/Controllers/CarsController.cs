using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Observers;


namespace WebApplication1.Models
{ 

    public class CarsController : Controller
    {
        private readonly AppDbContext _context; // підключаємо посилання на контекст бази даних

        public CarsController(AppDbContext context)
        {
            _context = context;
        }

       
        public IActionResult Index()
        {
            // Перевірка на авторизацію
            if (HttpContext.Session.GetString("UserLogin") == null)
            {
                return RedirectToAction("Login", "Auth"); //Якщо ні,викидаємо клієнта на сторінку із авторизацією
            }

            var cars = _context.Cars.ToList();
            return View(cars);
        }

        //поки що не реалізовано
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var car = await _context.Cars.FirstOrDefaultAsync(m => m.Id == id);
            if (car == null) return NotFound();

            return View(car);
        }

       
        public IActionResult Create()
        {
            return View();
        }

       
        [HttpPost] // означає, що метод буде викликатись тільки при HTTP-запиті типу POST (наприклад, коли надсилається форма).
        [ValidateAntiForgeryToken] // перевіряє спеціальний захисний токен, щоб запобігти CSRF-атакам (підробленій відправці форми з іншого сайту).
        public async Task<IActionResult> Create(Car car, List<IFormFile> files)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            List<string> savedFiles = new List<string>();

            try
            {
                // 1. Збереження файлу
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                            var path = Path.Combine(Directory.GetCurrentDirectory(),
                                "wwwroot/images/cars", fileName);

                            using (var stream = new FileStream(path, FileMode.Create))
                                await file.CopyToAsync(stream);

                            savedFiles.Add(path);
                        }
                    }

                    car.ImagePaths = string.Join(",", savedFiles
                        .Select(Path.GetFileName));
                }

                // Збереження моделі
                _context.Add(car);
                await _context.SaveChangesAsync();

                var subject = new CarSubject(); // observer pattern
                subject.Attach(new CarNewsLogger());
                subject.Notify(car);

                // Завершнення транзакції
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                //Якщо в бд не зберіглось,видаляємо записи
                foreach (var file in savedFiles)
                    if (System.IO.File.Exists(file))
                        System.IO.File.Delete(file);

                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
                return NotFound();

            return View(car);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Model,Year")] Car car)
        {
            if (id != car.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(car);

            try
            {
                _context.Update(car);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Cars.Any(e => e.Id == car.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = "Автомобіль успішно відредаговано!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var car = await _context.Cars
                .FirstOrDefaultAsync(m => m.Id == id);

            if (car == null)
                return NotFound();

            return View(car);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasOrders = await _context.Orders.AnyAsync(o => o.CarId == id);

            if (hasOrders)
            {
                TempData["ErrorMessage"] = "Неможливо видалити автомобіль, тому що на нього є замовлення.";
                return RedirectToAction(nameof(Index));
            }
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound();

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Автомобіль успішно видалено!";
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> LinqDemo()
        {
            // 1. Фільтрація
            var filteredCars = await _context.Cars
                .Where(c => c.Year >= 2020)
                .ToListAsync();

            // 2. Сортування
            var sortedCars = await _context.Cars
                .OrderByDescending(c => c.Price)
                .ToListAsync();

            // 3. Проєкція
            var projectedCars = await _context.Cars
                .Select(c => new CarProjectionViewModel
                {
                    Brand = c.Brand,
                    Model = c.Model,
                    Year = c.Year,
                    Price = c.Price
                })
                .ToListAsync();

            // 4. Пов'язані дані
            var orders = await _context.Orders
                .Include(o => o.Car)
                .Include(o => o.User)
                .ToListAsync();

            // 5. Агрегування
            var carsCount = await _context.Cars.CountAsync();

            var viewModel = new LinqDemoViewModel
            {
                FilteredCars = filteredCars,
                SortedCars = sortedCars,
                ProjectedCars = projectedCars,
                Orders = orders,
                CarsCount = carsCount
            };

            return View(viewModel);
        }
        public async Task<IActionResult> TestQuery()
        {
            var sw = new Stopwatch();

            // 1 — послідовний
            sw.Start();
            var sequential = GetCarsSequential();
            sw.Stop();
            ViewBag.SequentialTime = sw.ElapsedMilliseconds;

            // 2 — PLINQ
            sw.Restart();
            var plinq = GetCarsParallel();
            sw.Stop();
            
            ViewBag.PlinqTime = sw.ElapsedMilliseconds;
            ViewBag.Plinq = plinq;
            // 3 — Parallel.ForEach
            sw.Restart();
            var pForEach = GetCarsParallelForEach();
            sw.Stop();
            ViewBag.ForEachTime = sw.ElapsedMilliseconds;

            // 4 — TPL
            sw.Restart();
            var tpl = await GetCarsTPL();
            sw.Stop();
            ViewBag.TplTime = sw.ElapsedMilliseconds;

            ViewBag.Sequential = sequential;
            ViewBag.Plinq = plinq; 
            ViewBag.ForEach = pForEach;
            ViewBag.Tpl = tpl;

            return View();
        }



        private List<Car> GetCarsSequential()
        {
            return _context.Cars
                .Where(c => c.Year > 2015)
                .OrderByDescending(c => c.Year)
                .ToList();
        }

        private List<Car> GetCarsParallel()
        {
            var cars = _context.Cars.ToList();

            return cars
                .AsParallel()
                .Where(c => c.Year > 2015)
                .OrderByDescending(c => c.Year)
                .ToList();
        }
        public IActionResult Generate100kCars()
        {
            List<Car> cars = new List<Car>();

            for (int i = 1; i <= 100000; i++)
            {
                cars.Add(new Car
                {
                    Brand = "BMW",
                    Model = "BMW " + i,
                    Year = 2016 + (i % 8), // 2016-2023 
                    Price = 20000 + (i % 10000),
                    Description = "Generated car number " + i,
                    ImagePaths = null
                });
            }

            _context.Cars.AddRange(cars);
            _context.SaveChanges();

            return Content("100 000 записів успішно додано!");
        }
        private List<Car> GetCarsParallelForEach()
        {
            var cars = _context.Cars.ToList();

            List<Car> filtered = new List<Car>();
            object locker = new object();

            Parallel.ForEach(cars, car =>
            {
                if (car.Year > 2015)
                {
                    lock (locker)
                    {
                        filtered.Add(car);
                    }
                }
            });

            return filtered.OrderByDescending(c => c.Year).ToList();
        }
        private async Task<List<Car>> GetCarsTPL()
        {
            var cars = _context.Cars.ToList();

            var tasks = cars
                .Where(c => c.Year > 2015)
                .Select(c => Task.Run(() => c))
                .ToList();

            var results = await Task.WhenAll(tasks);

            return results
                .OrderByDescending(c => c.Year)
                .ToList();
        }
    }
}
