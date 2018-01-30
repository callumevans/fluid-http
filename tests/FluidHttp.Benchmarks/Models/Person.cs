using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FluidHttp.Benchmarks.Models
{
    public class Person
    {        
        public string Name { get; set; }
            
        public int Age { get; set; }
            
        public List<Car> Cars { get; set; } = new List<Car>();
        
        public static readonly string PersonJson = ReadPersonJsonFile();
        
        public static readonly Person LowComplexity = new Person
        {
            Name = "Fred",
            Age = 24
        };        
        
        public static readonly Person MediumComplexity = new Person
        {
            Name = "Bill",
            Age = 33,
            Cars = new List<Car>
            {
                Car.Sample,
                Car.Sample,
                Car.Sample
            }
        };

        public static readonly Person HighComplexity = new Person
        {
            Name = "Bill",
            Age = 33,
            Cars = new List<Car>
            {
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample,
                Car.Sample
            }
        };
        
        private static string ReadPersonJsonFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "FluidHttp.Benchmarks.person.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}