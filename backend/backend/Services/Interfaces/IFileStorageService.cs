namespace backend.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveImage(IFormFile file, string folder);
        Task DeleteImage(string? imageUrl);
    }
}