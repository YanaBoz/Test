namespace Test.Core.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
