namespace FluidHttp.Benchmarks.Models
{
    public class Car
    {
        public string Make { get; set; }
            
        public decimal Cost { get; set; }
        
        public static readonly Car Sample = new Car
        {
            Make = "Make A",
            Cost = 23467.44m
        };
    }
}