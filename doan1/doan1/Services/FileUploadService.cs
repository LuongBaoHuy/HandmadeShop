using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace doan1.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task<List<string>> UploadFileToTwoLocationsAsync(IFormFile file, string folderName);
        bool DeleteFile(string filePath);
        bool IsValidImageFile(IFormFile file);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

        // Gốc thư mục dùng chung ShareUploads (nằm cạnh thư mục project)
        private readonly string _sharedUploadsPath;

        // Base URL muốn lưu trong DB
        private const string RequestBasePath = "/uploads";

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;

            // doan1\doan1 -> lên 2 cấp: HandmadeShop -> ShareUploads
            var handmadeRoot = Directory.GetParent(_environment.ContentRootPath)!.Parent!.FullName;
            _sharedUploadsPath = Path.Combine(handmadeRoot, "ShareUploads");
        }

        public async Task<List<string>> UploadFileToTwoLocationsAsync(IFormFile file, string folderName)
        {
            var resultPaths = new List<string>();
            if (file == null || file.Length == 0) return resultPaths;

            if (!IsValidImageFile(file))
                throw new InvalidOperationException("File không hợp lệ. Chỉ chấp nhận các file ảnh (.jpg, .jpeg, .png, .gif, .bmp)");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Lưu vào ShareUploads
            var uploadsFolder = Path.Combine(_sharedUploadsPath, folderName);
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về URL DB theo chuẩn /uploads/{folder}/{file}
            var webPath = $"{RequestBasePath}/{folderName}/{fileName}".Replace("\\", "/");
            // Trả về 1 phần tử (giữ nguyên chữ ký hàm; nếu code cũ lấy [0] vẫn chạy)
            resultPaths.Add(webPath);
            return resultPaths;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return string.Empty;

            if (!IsValidImageFile(file))
                throw new InvalidOperationException("File không hợp lệ. Chỉ chấp nhận các file ảnh (.jpg, .jpeg, .png, .gif, .bmp)");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Lưu vào ShareUploads
            var uploadsFolder = Path.Combine(_sharedUploadsPath, folderName);
            Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Trả về URL DB theo chuẩn /uploads/{folder}/{file}
            return $"{RequestBasePath}/{folderName}/{fileName}".Replace("\\", "/");
        }

        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return false;

            try
            {
                // Hỗ trợ xóa theo đường dẫn DB dạng /uploads/{...}
                var prefix = $"{RequestBasePath}/";
                if (filePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var relative = filePath.Substring(prefix.Length).Replace("/", Path.DirectorySeparatorChar.ToString());
                    var physical = Path.Combine(_sharedUploadsPath, relative);
                    if (File.Exists(physical))
                    {
                        File.Delete(physical);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }

            return false;
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension)) return false;

            if (file.Length > MaxFileSizeInBytes) return false;

            if (!file.ContentType.StartsWith("image/")) return false;

            return true;
        }
    }
}
