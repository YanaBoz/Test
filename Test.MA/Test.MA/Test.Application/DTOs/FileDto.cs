namespace Test.Application.DTOs
{
    public class FileDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
    }
}
