using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FileStorageAPI.Data.Entities
{
    [Table("CustomerFiles")]
    public class CustomerFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }
        public int Version { get; set; }
        public bool IsLatest { get; set; }
        public DateTime CreateDate { get; set; }

    }
}