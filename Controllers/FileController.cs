using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharmacy_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        // ✅ Extension hợp lệ (bỏ .bmp vì ít dùng và dễ bị lợi dụng)
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // ✅ Magic bytes: map từ mime-type → các byte đầu file thực tế
        // Đây là cách duy nhất chặn được file đổi đuôi
        private static readonly Dictionary<string, byte[]> MagicBytes = new()
        {
            { "image/jpeg",  new byte[] { 0xFF, 0xD8, 0xFF } },
            { "image/png",   new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            { "image/gif",   new byte[] { 0x47, 0x49, 0x46, 0x38 } },
            { "image/webp",  new byte[] { 0x52, 0x49, 0x46, 0x46 } }, // RIFF....WEBP
        };

        // Extension phải khớp với magic bytes (tránh .exe đổi thành .png)
        private static readonly Dictionary<string, string[]> MimeToExtensions = new()
        {
            { "image/jpeg",  new[] { ".jpg", ".jpeg" } },
            { "image/png",   new[] { ".png" } },
            { "image/gif",   new[] { ".gif" } },
            { "image/webp",  new[] { ".webp" } },
        };

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Đọc magic bytes và kiểm tra file thực sự là ảnh.
        /// Trả về mime-type nếu hợp lệ, null nếu không hợp lệ.
        /// </summary>
        private async Task<string?> DetectMimeTypeAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            var bytesRead = await stream.ReadAsync(header, 0, header.Length);

            if (bytesRead < 4) return null;

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return "image/jpeg";

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return "image/png";

            // GIF: 47 49 46 38
            if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                return "image/gif";

            // WebP: RIFF????WEBP
            if (bytesRead >= 12 &&
                header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return "image/webp";

            return null; // Không phải ảnh hợp lệ
        }

        /// <summary>
        /// Validate toàn bộ một file — dùng chung cho upload đơn và upload nhiều
        /// </summary>
        private async Task<(bool IsValid, string? Error, string? MimeType)> ValidateFileAsync(IFormFile file)
        {
            // 1. Kiểm tra rỗng
            if (file == null || file.Length == 0)
                return (false, "File không được để trống.", null);

            // 2. Kiểm tra kích thước (2MB)
            if (file.Length > 2 * 1024 * 1024)
                return (false, $"File '{file.FileName}' vượt quá giới hạn 2MB.", null);

            // 3. Kiểm tra extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return (false, $"Định dạng '{ext}' không được hỗ trợ. Chỉ chấp nhận: jpg, png, gif, webp.", null);

            // 4. Chặn path traversal (../../etc/passwd.jpg)
            var safeName = Path.GetFileName(file.FileName);
            if (safeName != file.FileName.Replace("/", "").Replace("\\", ""))
                return (false, "Tên file không hợp lệ.", null);

            // 5. ✅ Đọc magic bytes — kiểm tra nội dung thực của file
            var mimeType = await DetectMimeTypeAsync(file);
            if (mimeType == null)
            {
                _logger.LogWarning(
                    "Phát hiện file nghi ngờ | IP: {IP} | FileName: {FileName} | Size: {Size}",
                    HttpContext.Connection.RemoteIpAddress, file.FileName, file.Length);

                return (false, $"File '{file.FileName}' không phải ảnh hợp lệ. Có thể file bị đổi đuôi.", null);
            }

            // 6. ✅ Extension phải khớp với magic bytes
            //    Ví dụ: file thực là PNG nhưng đặt tên .jpg → từ chối
            if (!MimeToExtensions[mimeType].Contains(ext))
            {
                _logger.LogWarning(
                    "Extension không khớp magic bytes | IP: {IP} | FileName: {FileName} | Detected: {Mime}",
                    HttpContext.Connection.RemoteIpAddress, file.FileName, mimeType);

                return (false, $"Extension '{ext}' không khớp với nội dung thực của file. Phát hiện file giả mạo!", null);
            }

            return (true, null, mimeType);
        }

        /// <summary>
        /// Lấy đường dẫn thư mục upload an toàn
        /// </summary>
        private string GetUploadFolder()
        {
            var webRoot = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var uploadFolder = Path.Combine(webRoot, "uploads", "brands");

            if (!Directory.Exists(uploadFolder))
            {
                try
                {
                    Directory.CreateDirectory(uploadFolder);
                    _logger.LogInformation($"Created upload folder: {uploadFolder}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to create upload folder: {uploadFolder}");

                    var tempFolder = Path.Combine(Path.GetTempPath(), "pharmacy_uploads", "brands");
                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);

                    _logger.LogInformation($"Using fallback temp folder: {tempFolder}");
                    return tempFolder;
                }
            }

            return uploadFolder;
        }

        /// <summary>
        /// Upload ảnh cho thương hiệu hoặc sản phẩm
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                // ✅ Validate (extension + kích thước + magic bytes)
                var (isValid, error, mimeType) = await ValidateFileAsync(file);
                if (!isValid)
                    return BadRequest(new { success = false, message = error });

                // Tạo tên file mới hoàn toàn bằng GUID — không dùng tên gốc từ client
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";

                var uploadFolder = GetUploadFolder();
                var filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded: {FileName} | Type: {Mime} | Size: {Size}KB",
                    fileName, mimeType, file.Length / 1024);

                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var imageUrl = $"{baseUrl}/uploads/brands/{fileName}";

                return Ok(new
                {
                    success = true,
                    url = imageUrl,
                    fileName = fileName,
                    message = "Upload ảnh thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload image failed");
                return StatusCode(500, new { success = false, message = $"Lỗi khi upload ảnh: {ex.Message}" });
            }
        }

        /// <summary>
        /// Upload nhiều ảnh cùng lúc
        /// </summary>
        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { success = false, message = "Vui lòng chọn ít nhất một file ảnh" });

                var uploadedFiles = new List<object>();
                var skippedFiles = new List<string>(); // track file bị từ chối
                var uploadFolder = GetUploadFolder();

                foreach (var file in files)
                {
                    // ✅ Validate từng file — bỏ qua file lỗi, không dừng toàn bộ
                    var (isValid, error, mimeType) = await ValidateFileAsync(file);
                    if (!isValid)
                    {
                        skippedFiles.Add($"{file.FileName}: {error}");
                        continue;
                    }

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var imageUrl = $"{baseUrl}/uploads/brands/{fileName}";

                    uploadedFiles.Add(new
                    {
                        url = imageUrl,
                        fileName = fileName,
                        originalName = file.FileName
                    });
                }

                // Nếu không có file nào upload được
                if (uploadedFiles.Count == 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không có file nào hợp lệ.",
                        skipped = skippedFiles
                    });

                return Ok(new
                {
                    success = true,
                    files = uploadedFiles,
                    message = $"Upload thành công {uploadedFiles.Count} file",
                    skipped = skippedFiles // thông báo file nào bị từ chối và lý do
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload multiple images failed");
                return StatusCode(500, new { success = false, message = $"Lỗi khi upload ảnh: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa ảnh — chặn path traversal
        /// </summary>
        [HttpDelete("delete")]
        public IActionResult DeleteImage([FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ" });

                // ✅ Chặn path traversal: chỉ lấy tên file, bỏ hết path
                // Hacker có thể gửi fileName = "../../appsettings.json"
                var safeFileName = Path.GetFileName(fileName);
                if (safeFileName != fileName)
                {
                    _logger.LogWarning("Path traversal attempt | IP: {IP} | FileName: {FileName}",
                        HttpContext.Connection.RemoteIpAddress, fileName);
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ." });
                }

                // ✅ Chỉ cho xóa file có extension là ảnh
                var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                    return BadRequest(new { success = false, message = "Chỉ có thể xóa file ảnh." });

                var uploadFolder = GetUploadFolder();
                var filePath = Path.Combine(uploadFolder, safeFileName);

                // ✅ Đảm bảo file nằm trong thư mục upload, không phải chỗ khác
                var fullPath = Path.GetFullPath(filePath);
                var fullFolder = Path.GetFullPath(uploadFolder);
                if (!fullPath.StartsWith(fullFolder))
                {
                    _logger.LogWarning("Path traversal attempt blocked | IP: {IP} | Path: {Path}",
                        HttpContext.Connection.RemoteIpAddress, fullPath);
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ." });
                }

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation("File deleted: {FileName}", safeFileName);
                    return Ok(new { success = true, message = "Xóa ảnh thành công" });
                }

                return NotFound(new { success = false, message = "Không tìm thấy file" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete image failed");
                return StatusCode(500, new { success = false, message = $"Lỗi khi xóa ảnh: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy ảnh mặc định nếu không có
        /// </summary>
        [HttpGet("default-image")]
        [AllowAnonymous]
        public IActionResult GetDefaultImage()
        {
            var uploadFolder = GetUploadFolder();
            var defaultImagePath = Path.Combine(uploadFolder, "default-brand.png");

            if (!System.IO.File.Exists(defaultImagePath))
            {
                var base64Image = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='3' width='18' height='18' rx='2' ry='2'%3E%3C/rect%3E%3Ccircle cx='8.5' cy='8.5' r='1.5'%3E%3C/circle%3E%3Cpolyline points='21 15 16 10 5 21'%3E%3C/polyline%3E%3C/svg%3E";
                return Ok(new { success = true, url = base64Image });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/uploads/brands/default-brand.png";
            return Ok(new { success = true, url = imageUrl });
        }
    }
}