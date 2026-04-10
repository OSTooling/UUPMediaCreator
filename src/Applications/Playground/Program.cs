namespace Playground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(new Windows.Internal.Flighting.PlatformCTAC("WU_OS", "10.0.26058.1000").UriQuery);
        }
    }
}