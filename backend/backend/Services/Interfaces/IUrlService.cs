namespace backend.Services.Interfaces
{
    public interface IUrlService
    {
        string GetAbsoluteUrl(string? path);
    }
}