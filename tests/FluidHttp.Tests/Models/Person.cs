using System.Collections.Generic;

namespace FluidHttp.Tests.Models
{
    public class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
        
        public List<Car> Cars { get; set; }

        public static readonly Person TestPerson = new Person
        {
            Name = "Test Person",
            Age = 33,
            Cars = new List<Car>
            {
                new Car {Make = "Make A", Cost = 111.00m},
                new Car {Make = "Make B", Cost = 99.99m}
            }
        };
    }
}