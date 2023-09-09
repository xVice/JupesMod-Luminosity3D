using Luminosity3D.Utils;

namespace JupesMod
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var engine = LD.StartEngine();

            Console.ReadKey();
            Logger.Log("Closing..");

            engine.StopEngine();

        }
    }
}