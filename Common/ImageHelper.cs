using Microsoft.AspNetCore.Http;

namespace CostumeRentalSystem.Common
{
    public static class ImageHelper
    {
        private const string UploadDirectory = "images/costumes";

        public static async Task<string> UploadImageAsync(IFormFile file, string webRootPath)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Създаваме уникално име на файла (GUID), за да избегнем дублиране
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // 2. Дефинираме пълния път до папката
            string folderPath = Path.Combine(webRootPath, UploadDirectory);

            // 3. Проверяваме дали папката съществува, ако не - я създаваме
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            // 4. Записваме файла физически
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Връщаме пътя, който ще се запише в базата данни (напр. /images/costumes/uuid.jpg)
            return "/" + UploadDirectory + "/" + fileName;
        }

        /// <summary>
        /// Изтрива файл от сървъра (използва се при премахване на костюм).
        /// </summary>
        public static void DeleteImage(string imagePath, string webRootPath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            // Премахваме водещата наклонена черта, ако я има
            string relativePath = imagePath.TrimStart('/');
            string fullPath = Path.Combine(webRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}