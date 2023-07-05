
namespace ICFP2023
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Console.WriteLine("I see these problems:");

            foreach (string path in ProblemCatalog.Instance.Names)
            {
                Console.WriteLine(path);
            }

            ConsoleSettings settings = new();
            settings.ParseArgs(args);
            Console.WriteLine($"Current: {settings}");
            Console.WriteLine($"Default: {new ConsoleSettings()}");
        }
    }
}