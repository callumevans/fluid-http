using BenchmarkDotNet.Running;

namespace FluidHttp.Benchmarks
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SerialisationBenchmarks>();
        }
    }
}