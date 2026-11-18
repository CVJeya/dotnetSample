// DESIGN PATTERN: Observer: notifies subscribed listeners when a publisher's state or event changes.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    // Step 1: Observer interface
    public interface IObserver
    {
        void Update(string message);
    }

    // Step 2: Publisher maintains a list of observers
    public class EventPublisher
    {
        private readonly List<IObserver> _observers = new();

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers(string message)
        {
            foreach (var observer in _observers)
            {
                observer.Update(message);
            }
        }
    }

    // Step 3: Email subscriber
    public class EmailSubscriber : IObserver
    {
        private readonly string _email;

        public EmailSubscriber(string email)
        {
            _email = email;
        }

        public void Update(string message)
        {
            Console.WriteLine($"Email sent to {_email}: {message}");
        }
    }

    // Step 4: Slack subscriber
    public class SlackSubscriber : IObserver
    {
        private readonly string _channel;

        public SlackSubscriber(string channel)
        {
            _channel = channel;
        }

        public void Update(string message)
        {
            Console.WriteLine($"Slack message in {_channel}: {message}");
        }
    }

    // Step 5: Execute the demo
    public class ObserverPattern
    {
        public static void Run()
        {
            var publisher = new EventPublisher();

            IObserver email = new EmailSubscriber("user@example.com");
            IObserver slack = new SlackSubscriber("#notifications");

            publisher.Attach(email);
            publisher.Attach(slack);

            publisher.NotifyObservers("New order received!");

            Console.WriteLine("\nRemoving email subscriber...");

            publisher.Detach(email);

            publisher.NotifyObservers("Another order received!");
        }
    }
}