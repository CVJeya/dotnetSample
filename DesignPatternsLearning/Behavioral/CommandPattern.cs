// DESIGN PATTERN: Command: represents an action as an object, enabling queuing, logging, and undo behavior.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class Light
    {
        public void TurnOn()
        {
            Console.WriteLine("Light is ON");
        }

        public void TurnOff()
        {
            Console.WriteLine("Light is OFF");
        }
    }

    public class LightOnCommand : ICommand
    {
        private readonly Light _light;

        public LightOnCommand(Light light)
        {
            _light = light;
        }

        public void Execute() => _light.TurnOn();

        public void Undo() => _light.TurnOff();
    }

    public class LightOffCommand : ICommand
    {
        private readonly Light _light;

        public LightOffCommand(Light light)
        {
            _light = light;
        }

        public void Execute() => _light.TurnOff();

        public void Undo() => _light.TurnOn();
    }

    public class RemoteControl
    {
        private readonly Stack<ICommand> _history = new();

        public void PressButton(ICommand command)
        {
            command.Execute();
            _history.Push(command);
        }

        public void PressUndo()
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("Nothing to undo");
                return;
            }

            ICommand command = _history.Pop();
            command.Undo();
        }
    }

    public class CommandPattern
    {
        public static void Run()
        {
            var light = new Light();
            var remote = new RemoteControl();

            remote.PressButton(new LightOnCommand(light));
            remote.PressButton(new LightOffCommand(light));

            remote.PressUndo();
        }
    }
}
