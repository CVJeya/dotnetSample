// DESIGN PATTERN: Memento: captures an object's state so it can be restored later without exposing its internals.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    // Stores the editor's saved state
    public class Memento
    {
        public string State { get; }

        public Memento(string state)
        {
            State = state;
        }
    }

    // Object whose state we want to save
    public class Editor
    {
        private string _content = "";

        public void Type(string text)
        {
            _content += text;
            Console.WriteLine($"Content: {_content}");
        }

        public Memento SaveState()
        {
            return new Memento(_content);
        }

        public void RestoreState(Memento memento)
        {
            _content = memento.State;
            Console.WriteLine($"Restored to: {_content}");
        }
    }

    // Maintains the saved states
    public class EditorHistory
    {
        private readonly Stack<Memento> _history = new();

        public void Save(Editor editor)
        {
            _history.Push(editor.SaveState());
        }

        public void Undo(Editor editor)
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("No saved state available");
                return;
            }

            Memento memento = _history.Pop();
            editor.RestoreState(memento);
        }
    }

    public class MementoPattern
    {
        public static void Run()
        {
            var editor = new Editor();
            var history = new EditorHistory();

            editor.Type("Hello ");

            history.Save(editor);

            editor.Type("World");

            history.Save(editor);

            editor.Type("!");

            // Undo last saved change
            history.Undo(editor);

            // Undo previous saved change
            history.Undo(editor);
        }
    }
}
