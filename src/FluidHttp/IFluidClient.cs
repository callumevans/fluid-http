using System.Threading.Tasks;

namespace FluidHttp
{
    public interface IFluidClient
    {
        string BaseUrl { get; }
        
        bool BaseUrlSet { get; }

        void SetDefaultHeader(string name, string value);

        Task<IFluidResponse> FetchAsync(IFluidRequest request);
    }
}
