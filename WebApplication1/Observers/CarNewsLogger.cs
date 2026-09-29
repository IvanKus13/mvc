using WebApplication1.Models;

namespace WebApplication1.Observers
{
    public class CarNewsLogger : ICarObserver
    {

        
        public void Update(Car car)
        {
            string news =
                $" [{DateTime.Now}] Нова машина додана: {car.Brand} {car.Model} | Рік: {car.Year} | Ціна: {car.Price}\n";

            string path = "New_cars.txt";

            File.AppendAllText(path, news);

           
        }
    }
}