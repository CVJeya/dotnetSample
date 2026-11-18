// DESIGN PATTERN: Template Method: defines an algorithm's skeleton and lets subclasses customize selected steps.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public abstract class DataProcessor
    {
        // Template method
        // Child classes cannot override this method.
        public void Process()
        {
            OpenFile();
            ExtractData();
            TransformData();
            ValidateData();
            SaveData();
        }

        protected abstract void OpenFile();

        protected abstract void ExtractData();

        protected abstract void TransformData();

        protected abstract void ValidateData();

        protected abstract void SaveData();
    }

    public class CsvDataProcessor : DataProcessor
    {
        protected override void OpenFile()
        {
            Console.WriteLine("Opening CSV file");
        }

        protected override void ExtractData()
        {
            Console.WriteLine("Extracting data from CSV");
        }

        protected override void TransformData()
        {
            Console.WriteLine("Transforming CSV data");
        }

        protected override void ValidateData()
        {
            Console.WriteLine("Validating CSV data");
        }

        protected override void SaveData()
        {
            Console.WriteLine("Saving CSV data to database");
        }
    }

    public class JsonDataProcessor : DataProcessor
    {
        protected override void OpenFile()
        {
            Console.WriteLine("Opening JSON file");
        }

        protected override void ExtractData()
        {
            Console.WriteLine("Extracting data from JSON");
        }

        protected override void TransformData()
        {
            Console.WriteLine("Transforming JSON data");
        }

        protected override void ValidateData()
        {
            Console.WriteLine("Validating JSON data");
        }

        protected override void SaveData()
        {
            Console.WriteLine("Saving JSON data to database");
        }
    }

    public class TemplateMethodPattern
    {
        public static void Run()
        {
            DataProcessor csv = new CsvDataProcessor();
            csv.Process();

            Console.WriteLine();

            DataProcessor json = new JsonDataProcessor();
            json.Process();
        }
    }
}
