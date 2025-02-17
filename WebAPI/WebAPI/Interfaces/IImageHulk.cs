namespace WebAPI.Interfaces
{
    public interface IImageHulk
    {
        Task<string> Save(IFormFile image);
        Task<string> Save(string urlImage);
        bool Delete(string fileName);
    }
}
