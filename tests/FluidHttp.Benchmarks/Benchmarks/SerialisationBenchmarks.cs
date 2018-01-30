using BenchmarkDotNet.Attributes;
using FluidHttp.Benchmarks.Models;
using RestSharp;

namespace FluidHttp.Benchmarks
{
    public class SerialisationBenchmarks
    {
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseJson_LowComplexity()
        {
            var output = new FluidRequest();
            output.WithJsonBody(Person.LowComplexity);
            return output;
        }
        
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseJson_MediumComplexity()
        {            
            var output = new FluidRequest();
            output.WithJsonBody(Person.MediumComplexity);
            return output;
        }
                        
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseJson_HighComplexity()
        {            
            var output = new FluidRequest();
            output.WithJsonBody(Person.HighComplexity);
            return output;
        }
        
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseXml_LowComplexity()
        {
            var output = new FluidRequest();
            output.WithXmlBody(Person.LowComplexity);
            return output;
        }
        
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseXml_MediumComplexity()
        {            
            var output = new FluidRequest();
            output.WithXmlBody(Person.MediumComplexity);
            return output;
        }
                        
        [Benchmark]
        public IFluidRequest FluidHttp_SerialiseXml_HighComplexity()
        {            
            var output = new FluidRequest();
            output.WithXmlBody(Person.HighComplexity);
            return output;
        }
                
        [Benchmark]
        public IRestRequest RestSharp_SerialiseJson_LowComplexity()
        {
            var output = new RestRequest();
            output.AddJsonBody(Person.LowComplexity);
            return output;
        }        
        
        [Benchmark]
        public IRestRequest RestSharp_SerialiseJson_MediumComplexity()
        {
            var output = new RestRequest();
            output.AddJsonBody(Person.MediumComplexity);
            return output;
        }    
        
        [Benchmark]
        public IRestRequest RestSharp_SerialiseJson_HighComplexity()
        {
            var output = new RestRequest();
            output.AddJsonBody(Person.HighComplexity);
            return output;
        }      
        
        [Benchmark]
        public IRestRequest RestSharp_SerialiseXml_LowComplexity()
        {
            var output = new RestRequest();
            output.AddXmlBody(Person.LowComplexity);
            return output;
        }        
        
        [Benchmark]
        public IRestRequest RestSharp_SerialiseXml_MediumComplexity()
        {
            var output = new RestRequest();
            output.AddXmlBody(Person.MediumComplexity);
            return output;
        }    
        
        [Benchmark]
        public IRestRequest RestSharp_SerialiseXml_HighComplexity()
        {
            var output = new RestRequest();
            output.AddXmlBody(Person.HighComplexity);
            return output;
        }
    }
}