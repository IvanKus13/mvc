namespace WebApplication1.Models
{
    public class LinqDemoViewModel
    {
        public List<Car> FilteredCars { get; set; } = new();

        public List<Car> SortedCars { get; set; } = new();

        public List<CarProjectionViewModel> ProjectedCars { get; set; } = new();

        public List<Order> Orders { get; set; } = new();

        public int CarsCount { get; set; }
    }

    public class CarProjectionViewModel
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
    }
}