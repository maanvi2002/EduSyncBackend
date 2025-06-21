using Microsoft.AspNetCore.Http;

namespace EduSyncProject.DTO
{
    public class CourseUpdateWithFileDTO
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}
