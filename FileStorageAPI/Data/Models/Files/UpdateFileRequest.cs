using System.ComponentModel.DataAnnotations;

namespace FileStorageAPI.Data.Models.Files
{
    public class UpdateFileRequest
    {
        [Required]
        public byte[] Data { get; set; }
    }
}