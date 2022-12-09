namespace FileStorageAPI.Helpers
{
    public class Utility
    {
        public static byte[] GetFileBytes(IFormFile file)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                file.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
