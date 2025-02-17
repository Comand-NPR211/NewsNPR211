using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using WebAPI.Interfaces;

namespace WebAlina.Services
{
    public class ImageHulk : IImageHulk
    {
        private readonly IConfiguration _configuration;
        public ImageHulk(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public bool Delete(string fileName)
        {
            try
            {
                var dir = _configuration["ImageFolder"];
                var sizes = _configuration["ImageSizes"].Split(",")
                    .Select(x => int.Parse(x));
                //int[] sizes = [50, 150, 300, 600, 1200];
                foreach (var size in sizes)
                {
                    string dirSave = Path.Combine(Directory.GetCurrentDirectory(),
                        dir, $"{size}_{fileName}");

                    if (File.Exists(dirSave))
                        File.Delete(dirSave);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> Save(IFormFile image)
        {
            string imageName = String.Empty;

            using (MemoryStream ms = new())
            {
                await image.CopyToAsync(ms);
                var bytes = ms.ToArray();
                imageName = SaveByteArray(bytes);
            }

            return imageName;
        }

        private string SaveByteArray(byte[] bytes)
        {
            string imageName = Guid.NewGuid().ToString() + ".webp";
            var dir = _configuration["ImageFolder"];

            var sizes = _configuration["ImageSizes"].Split(",")
                    .Select(x => int.Parse(x));
            //int[] sizes = [50, 150, 300, 600, 1200];
            foreach (var size in sizes)
            {
                string dirSave = Path.Combine(Directory.GetCurrentDirectory(),
                    dir, $"{size}_{imageName}");
                using (var imageLoad = Image.Load(bytes))
                {
                    // Resize the image (50% of original dimensions)
                    imageLoad.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(size, size),
                        Mode = ResizeMode.Max
                    }));

                    // Save the image with compression
                    imageLoad.Save(dirSave, new WebpEncoder());
                }
            }
            return imageName;
        }


        public async Task<string> Save(string urlImage)
        {
            string imageName = String.Empty;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Send a GET request to the image URL
                    HttpResponseMessage response = client.GetAsync(urlImage).Result;

                    // Check if the response status code indicates success (e.g., 200 OK)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the image bytes from the response content
                        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
                        imageName = SaveByteArray(imageBytes);
                    }
                }
            }
            catch
            {
                return imageName;
            }
            return imageName;
        }
    }
}