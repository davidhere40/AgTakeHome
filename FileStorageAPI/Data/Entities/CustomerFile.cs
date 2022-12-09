using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageAPI.Data.Entities
{
    /// <summary>
    /// Entity to store uploaded Customer File Data
    /// </summary>
    [Table("CustomerFiles")]
    public class CustomerFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FileName { get; set; }
        /// <summary>
        /// Binary Data of the file
        /// </summary>
        public byte[] Data { get; set; }
        public int Version { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
    }
}