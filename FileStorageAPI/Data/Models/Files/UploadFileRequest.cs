using System.ComponentModel.DataAnnotations;

namespace FileStorageAPI.Data.Models.Files
{
    public class UploadFileRequest
    {
        [Required]
        public byte[] Data { get; set; }
    }
}