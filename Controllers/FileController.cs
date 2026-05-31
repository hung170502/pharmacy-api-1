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

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Lấy đường dẫn thư mục upload an toàn
        /// </summary>
        private string GetUploadFolder()
        {
            // Thử dùng WebRootPath trước
            var webRoot = _environment.WebRootPath;

            // Nếu WebRootPath null (trên Render), dùng ContentRootPath
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadFolder = Path.Combine(webRoot, "uploads", "brands");

            // Đảm bảo thư mục tồn tại
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

                    // Fallback: dùng thư mục temp nếu không tạo được
                    var tempFolder = Path.Combine(Path.GetTempPath(), "pharmacy_uploads", "brands");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }
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
                // Kiểm tra file có tồn tại không
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn file ảnh"
                    });
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Định dạng file không được hỗ trợ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP, BMP"
                    });
                }

                // Kiểm tra kích thước file (tối đa 2MB)
                if (file.Length > 2 * 1024 * 1024)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Kích thước file không được vượt quá 2MB"
                    });
                }

                // Tạo tên file duy nhất
                var fileName = $"{Guid.NewGuid()}{extension}";

                // Lấy thư mục upload (có xử lý fallback)
                var uploadFolder = GetUploadFolder();
                var filePath = Path.Combine(uploadFolder, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {fileName}");

                // Tạo URL trả về
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
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi upload ảnh: {ex.Message}"
                });
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
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn ít nhất một file ảnh"
                    });
                }

                var uploadedFiles = new List<object>();
                var uploadFolder = GetUploadFolder();

                foreach (var file in files)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        continue; // Bỏ qua file không hợp lệ
                    }

                    // Kiểm tra kích thước file (tối đa 2MB)
                    if (file.Length > 2 * 1024 * 1024)
                    {
                        continue; // Bỏ qua file quá lớn
                    }

                    // Tạo tên file duy nhất
                    var fileName = $"{Guid.NewGuid()}{extension}";
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

                return Ok(new
                {
                    success = true,
                    files = uploadedFiles,
                    message = $"Upload thành công {uploadedFiles.Count} file"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload multiple images failed");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi upload ảnh: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Xóa ảnh
        /// </summary>
        [HttpDelete("delete")]
        public IActionResult DeleteImage([FromQuery] string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ" });
                }

                var uploadFolder = GetUploadFolder();
                var filePath = Path.Combine(uploadFolder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"File deleted successfully: {fileName}");
                    return Ok(new { success = true, message = "Xóa ảnh thành công" });
                }

                return NotFound(new { success = false, message = "Không tìm thấy file" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete image failed");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi khi xóa ảnh: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Lấy ảnh mặc định nếu không có
        /// </summary>
        [HttpGet("default-image")]
        public IActionResult GetDefaultImage()
        {
            var uploadFolder = GetUploadFolder();
            var defaultImagePath = Path.Combine(uploadFolder, "default-brand.png");

            // Nếu không có ảnh mặc định, tạo placeholder
            if (!System.IO.File.Exists(defaultImagePath))
            {
                // Trả về base64 image placeholder
                var base64Image = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100' viewBox='0 0 24 24' fill='none' stroke='%23999' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='3' width='18' height='18' rx='2' ry='2'%3E%3C/rect%3E%3Ccircle cx='8.5' cy='8.5' r='1.5'%3E%3C/circle%3E%3Cpolyline points='21 15 16 10 5 21'%3E%3C/polyline%3E%3C/svg%3E";

                return Ok(new { success = true, url = base64Image });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/uploads/brands/default-brand.png";

            return Ok(new { success = true, url = imageUrl });
        }
    }
}