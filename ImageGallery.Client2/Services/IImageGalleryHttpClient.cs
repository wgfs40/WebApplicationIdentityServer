using System.Net.Http;
using System.Threading.Tasks;

namespace ImageGallery.Client2.Services
{
    public interface IImageGalleryHttpClient
    {
        Task<HttpClient> GetClient();
    }
}
