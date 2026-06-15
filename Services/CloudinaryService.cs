using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using CloudinaryAccount = CloudinaryDotNet.Account; // THÊM DÒNG NÀY

namespace Pharmacy_API.Services;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var account = new CloudinaryAccount( // SỬA Account → CloudinaryAccount
            config["CloudinarySettings:CloudName"],
            config["CloudinarySettings:ApiKey"],
            config["CloudinarySettings:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string folder = "brands")
    {
        if (file == null || file.Length == 0) return null;

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation()
                .Width(800).Height(800).Crop("limit").Quality("auto")
        };

        return await _cloudinary.UploadAsync(uploadParams);
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId)) return false;
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        return result.Result == "ok";
    }
}