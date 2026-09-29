using WebApplication1.Models;

namespace WebApplication1.Observers
{
    public class CarSubject
    {
        private List<ICarObserver> observers = new();

        public void Attach(ICarObserver observer)
        {
            observers.Add(observer);
        }

        public void Notify(Car car)
        {
            foreach (var observer in observers)
            {
                observer.Update(car);
            }
        }
    }
}
