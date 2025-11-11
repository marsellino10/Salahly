using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalahlyProject.Services.Interfaces
{
    /// <summary>
    /// Service interface for file upload operations
    /// Handles file upload to cloud storage and URL generation
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Upload a file to cloud storage
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="uploadFolder">The folder name in cloud storage (e.g., "crafts")</param>
        /// <returns>The full URL of the uploaded file</returns>
        Task<string> UploadFileAsync(IFormFile file, string uploadFolder);

        /// <summary>
        /// Delete a file from cloud storage
        /// </summary>
        /// <param name="cloudinaryUrl">The full Cloudinary URL or public ID of the file</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteFileAsync(string cloudinaryUrl);

        /// <summary>
        /// Check if a file exists in cloud storage
        /// </summary>
        /// <param name="cloudinaryUrl">The full Cloudinary URL or public ID of the file</param>
        /// <returns>True if file exists, false otherwise</returns>
        Task<bool> FileExistsAsync(string cloudinaryUrl);

        /// <summary>
        /// Get the full URL for a file
        /// </summary>
        /// <param name="cloudinaryUrl">The full Cloudinary URL or public ID of the file</param>
        /// <returns>The full URL of the file</returns>
        string GetFileUrl(string cloudinaryUrl);

        /// <summary>
        /// Extract public ID from Cloudinary URL
        /// </summary>
        /// <param name="cloudinaryUrl">The full Cloudinary URL</param>
        /// <returns>The public ID (e.g., salahly/crafts_guid_filename)</returns>
        string ExtractPublicIdFromUrl(string cloudinaryUrl);
    }
}
