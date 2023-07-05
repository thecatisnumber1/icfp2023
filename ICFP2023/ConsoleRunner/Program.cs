using ICFP2023;

Console.WriteLine("Hello, World!");

Console.WriteLine("I see these problems:");

foreach (string path in ProblemCatalog.Instance.Names)
{
    Console.WriteLine(path);
}