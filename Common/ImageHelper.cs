using Microsoft.AspNetCore.Http;

namespace CostumeRentalSystem.Common
{
    public static class ImageHelper
    {
        private const string UploadDirectory = "images/costumes";

        public static async Task<string> UploadImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0) return null;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            string folderPath = Path.Combine(webRootPath, UploadDirectory);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/" + UploadDirectory + "/" + fileName;
        }

        public static void DeleteImage(string imagePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            string relativePath = imagePath.TrimStart('/');
            string fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}