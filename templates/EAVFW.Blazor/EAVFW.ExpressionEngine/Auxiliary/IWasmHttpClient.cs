using System.Net.Http;
using System.Threading.Tasks;

namespace EAVFW.ExpressionEngine.Auxiliary
{
    public interface IWasmHttpClient
    {
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage);
        Task<HttpResponseMessage> GetAsync(string uri);
    }
}