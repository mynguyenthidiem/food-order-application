using backend.Services.Interfaces;

namespace backend.Services
{
    public class UrlService : IUrlService
    {
        private readonly IHttpContextAccessor _accessor;

        public UrlService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string GetAbsoluteUrl(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var request = _accessor.HttpContext!.Request;

            return $"{request.Scheme}://{request.Host}{path}";
        }
    }
}