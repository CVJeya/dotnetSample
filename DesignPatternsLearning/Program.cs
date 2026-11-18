
using DesignPatternsLearning.Creational;
using DesignPatternsLearning.Structural;
using DesignPatternsLearning.Behavioral;
/*Complete Pattern Summary
#	Pattern	Purpose	Key Benefit
1	Singleton	One instance globally	Control resource access
2	Factory	Create objects dynamically	Hide creation complexity
3	Builder	Build complex objects	Step-by-step construction
4	Prototype	Clone objects	Avoid expensive creation
5	Object Pool	Reuse expensive objects	Improve performance
6	Adapter	Make incompatible interfaces work	Integrate legacy systems
7	Bridge	Decouple abstraction from implementation	Independent evolution
8	Composite	Treat objects uniformly in hierarchies	Simplify recursive structures
9	Decorator	Add functionality dynamically	Extend without modification
10	Facade	Simplify complex subsystems	Easy-to-use interface
11	Observer	Notify multiple listeners	Loose coupling
12	Strategy	Encapsulate algorithms	Choose algorithm at runtime
13	Command	Encapsulate requests as objects	Queue, undo, replay
14	Chain of Responsibility	Pass request through handlers	Dynamic handler selection
15	State	Change behavior by state	State-specific logic
16	Template Method	Define algorithm skeleton	Subclass customization
17	Memento	Capture/restore state	Undo functionality
18	Iterator	Access collection elements sequentially	Hide internal structure
19	Visitor	Perform operations on elements	Separate operations from data
20	Interpreter	Define grammar and parser	Domain-specific languages
21	Repository	Abstract data access	Swap storage backends
22	Unit of Work	Manage transactions	Atomic operations
23	CQRS	Separate reads from writes	Scalable queries
24	Mediator	Centralize communication	Reduce coupling
25	Specification	Encapsulate query logic	Reusable queries
26	Proxy	Control access to objects	Security, caching, lazy loading
27	DI Container	Manage dependencies	Automatic wiring
28	Pipeline/Middleware	Process through chain	Request handling
29	Interceptor	Intercept method calls	Logging, performance monitoring
30	Event Sourcing	Store state as events	Complete audit trail */

while (true)
{
    Console.WriteLine("\n=== Design Patterns Learning ===");
    Console.WriteLine("1. Singleton");
    Console.WriteLine("2. Factory");
    Console.WriteLine("3. Builder");
    Console.WriteLine("4. Prototype");
    Console.WriteLine("5. Object Pool");
    Console.WriteLine("6. Adapter");
    Console.WriteLine("7. Bridge");
    Console.WriteLine("8. Composite");
    Console.WriteLine("9. Decorator");
    Console.WriteLine("10. Facade");
   
    Console.WriteLine("===== BEHAVIOURAL DESIGN PATTERNS =====");

    Console.WriteLine("11. Observer");
    Console.WriteLine("12. Strategy");
    Console.WriteLine("13. Command");
    Console.WriteLine("14. Chain of Responsibility");
    Console.WriteLine("15. State");
    Console.WriteLine("16. Template Method");
    Console.WriteLine("17. Memento");
    Console.WriteLine("18. Iterator");
    Console.WriteLine("19. Visitor");
    Console.WriteLine("20. Interpreter");

    Console.WriteLine("0. Exit");
    Console.Write("Choose a pattern: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            SingletonDemo.Run();
            break;

        case "2":
            FactoryDemo.Run();
            break;
        case "3":
            BuilderDemo.Run();
            break;

        case "4":
            PrototypeDemo.Run();
            break;

        case "5":
            ObjectPoolDemo.Run();
            break;
        case "6":
            AdapterDemo.Run();
            break;

        case "7":
            BridgeDemo.Run();
            break;

        case "8":
            CompositeDemo.Run();
            break;

        case "9":
            DecoratorDemo.Run();
            break;

        case "10":
            FacadeDemo.Run();
            break;
        case "11":
            ObserverPattern.Run();
            break;

        case "12":
            StrategyPattern.Run();
            break;

        case "13":
            CommandPattern.Run();
            break;

        case "14":
            ChainOfResponsibilityPattern.Run();
            break;

        case "15": 
            StatePattern.Run();
            break;

        case "16":
            TemplateMethodPattern.Run();
            break;

        case "17":
            MementoPattern.Run();
            break;

        case "18":
            IteratorPattern.Run();
            break;

        case "19":
            VisitorPattern.Run();
            break;

        case "20":
            InterpreterPattern.Run();
            break;
        case "0":
            return;

        default:
            Console.WriteLine("Invalid choice. Try again.");
            break;
    }

    Console.WriteLine("\nPress Enter to continue...");
    Console.ReadLine();
}
