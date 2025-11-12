using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalahlyProject.Services.Interfaces;

namespace SalahlyProject.Services
{
    /// <summary>
    /// Cloudinary-based implementation for file upload operations
    /// Handles file storage on Cloudinary and URL generation
    /// Public ID is extracted from the Cloudinary URL when needed
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<FileUploadService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        // Allowed file extensions for validation
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileUploadService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                _logger.LogError("Cloudinary configuration is missing. Please set Cloudinary:CloudName, Cloudinary:ApiKey and Cloudinary:ApiSecret.");
                throw new InvalidOperationException("Cloudinary configuration is missing. Please configure Cloudinary credentials.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };

            _logger.LogInformation("Cloudinary initialized for cloud: {CloudName}", cloudName);
        }

        /// <summary>
        /// Upload a file to Cloudinary and return secure URL
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string uploadFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            if (file.Length > MaxFileSize)
                throw new ArgumentException($"File size exceeds maximum allowed size of {MaxFileSize / (1024 * 1024)} MB", nameof(file));

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Any(ext => ext == fileExtension))
                throw new ArgumentException($"File type '{fileExtension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}", nameof(file));

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var publicId = $"{uploadFolder}_{Guid.NewGuid():N}_{Path.GetFileNameWithoutExtension(file.FileName)}";

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, memoryStream),
                    PublicId = publicId,
                    Folder = $"salahly/{uploadFolder}",
                    Overwrite = false,
                    UseFilename = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult == null || uploadResult.Error != null)
                {
                    _logger.LogError(uploadResult?.Error?.Message ?? "Unknown error uploading to Cloudinary");
                    throw new InvalidOperationException($"Failed to upload file to Cloudinary: {uploadResult?.Error?.Message}");
                }

                _logger.LogInformation("File uploaded to Cloudinary: {PublicId} (URL: {Url})", uploadResult.PublicId, uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString());

                return uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? uploadResult.PublicId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Cloudinary: {FileName}", file?.FileName);
                throw;
            }
        }

        /// <summary>
        /// Delete a file from Cloudinary by URL or public ID
        /// Extracts public ID from URL if necessary
        /// </summary>
        public async Task<bool> DeleteFileAsync(string cloudinaryUrl)
        {
            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
            {
                _logger.LogWarning("DeleteFileAsync called with empty cloudinary URL");
                return false;
            }

            try
            {
                // Extract public ID from URL if it's a full URL
                var publicId = ExtractPublicIdFromUrl(cloudinaryUrl);

                if (string.IsNullOrWhiteSpace(publicId))
                {
                    _logger.LogWarning("Could not extract public ID from URL: {Url}", cloudinaryUrl);
                    return false;
                }

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                if (deleteResult == null)
                {
                    _logger.LogWarning("Cloudinary returned null for delete operation on {PublicId}", publicId);
                    return false;
                }

                if (deleteResult.Error != null)
                {
                    _logger.LogWarning("Cloudinary delete error: {Error}", deleteResult.Error.Message);
                    return false;
                }

                var success = string.Equals(deleteResult.Result, "ok", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(deleteResult.Result, "deleted", StringComparison.OrdinalIgnoreCase);

                if (success)
                    _logger.LogInformation("File deleted successfully from Cloudinary: {PublicId}", publicId);
                else
                    _logger.LogWarning("Cloudinary delete returned result '{Result}' for {PublicId}", deleteResult.Result, publicId);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Cloudinary: {Url}", cloudinaryUrl);
                return false;
            }
        }

        /// <summary>
        /// Check if a file exists in Cloudinary by URL or public ID
        /// Extracts public ID from URL if necessary
        /// </summary>
        public async Task<bool> FileExistsAsync(string cloudinaryUrl)
        {
            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                return false;

            try
            {
                // Extract public ID from URL if it's a full URL
                var publicId = ExtractPublicIdFromUrl(cloudinaryUrl);

                if (string.IsNullOrWhiteSpace(publicId))
                {
                    _logger.LogWarning("Could not extract public ID from URL: {Url}", cloudinaryUrl);
                    return false;
                }

                var getParams = new GetResourceParams(publicId)
                {
                    ResourceType = ResourceType.Image
                };

                var resource = await _cloudinary.GetResourceAsync(getParams);

                var exists = resource != null && !string.IsNullOrWhiteSpace(resource.PublicId);
                if (exists)
                    _logger.LogInformation("File exists in Cloudinary: {PublicId}", publicId);
                else
                    _logger.LogWarning("File not found in Cloudinary: {PublicId}", publicId);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence in Cloudinary: {Url}", cloudinaryUrl);
                return false;
            }
        }

        /// <summary>
        /// Get the full URL for a Cloudinary file
        /// If already a full URL, returns as-is; otherwise generates from public ID
        /// </summary>
        public string GetFileUrl(string cloudinaryUrl)
        {
            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                return null;

            // If already a full URL, return it
            if (cloudinaryUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || cloudinaryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return cloudinaryUrl;

            try
            {
                // Generate URL from public ID
                var url = _cloudinary.Api.UrlImgUp
                    .Secure(true)
                    .Transform(new Transformation().Quality("auto"))
                    .BuildUrl(cloudinaryUrl);

                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating file URL for: {Url}", cloudinaryUrl);
                return null;
            }
        }

        /// <summary>
        /// Extract public ID from Cloudinary URL
        /// URL format: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{public_id}
        /// Public ID includes the folder: salahly/crafts_{guid}_{filename}
        /// </summary>
        public string ExtractPublicIdFromUrl(string cloudinaryUrl)
        {
            if (string.IsNullOrWhiteSpace(cloudinaryUrl))
                return null;

            try
            {
                // If it's already just a public ID (not a full URL), return it as-is
                if (!cloudinaryUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !cloudinaryUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return cloudinaryUrl;
                }

                const string uploadSegment = "/upload/";
                var uploadIndex = cloudinaryUrl.IndexOf(uploadSegment, StringComparison.OrdinalIgnoreCase);

                if (uploadIndex == -1)
                {
                    _logger.LogWarning("Could not find '/upload/' segment in Cloudinary URL: {Url}", cloudinaryUrl);
                    return null;
                }

                // Start after "/upload/"
                var startIndex = uploadIndex + uploadSegment.Length;
                var remainder = cloudinaryUrl.Substring(startIndex);

                // Skip version if present (e.g., "v1234567890/")
                if (remainder.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                {
                    var versionEndIndex = remainder.IndexOf('/');
                    if (versionEndIndex != -1)
                    {
                        remainder = remainder.Substring(versionEndIndex + 1);
                    }
                }

                // Remove query params/fragments
                var publicId = remainder.Split(new[] { '?', '#' }, StringSplitOptions.None)[0];

                // Decode URL-encoded characters
                publicId = System.Net.WebUtility.UrlDecode(publicId);

                //  Remove file extension (.png, .jpg, .jpeg, .gif, .webp, etc.)
                var lastDotIndex = publicId.LastIndexOf('.');
                if (lastDotIndex != -1)
                {
                    var extension = publicId.Substring(lastDotIndex + 1).ToLowerInvariant();
                    var knownExtensions = new[] { "jpg", "jpeg", "png", "gif", "bmp", "tiff", "webp", "svg", "heic", "heif" };

                    if (knownExtensions.Contains(extension))
                    {
                        publicId = publicId.Substring(0, lastDotIndex);
                    }
                }

                return !string.IsNullOrWhiteSpace(publicId) ? publicId : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting public ID from Cloudinary URL: {Url}", cloudinaryUrl);
                return null;
            }
        }

    }
}
