namespace EduSyncProject.DTO
{
    /// DTO for student operations (enrollment/unenrollment)
   
    [System.ComponentModel.DisplayName("Student Enrollment")]
    public class StudentEnrollmentDTO
    {
        public Guid CourseId { get; set; }
    }

    /// DTO for instructor operations (enrolling/unenrolling students)
    [System.ComponentModel.DisplayName("Instructor Student Enrollment")]
    public class InstructorEnrollmentDTO
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
    }

    public class EnrollmentReadDTO
    {
        public string CourseId { get; set; } = null!;
        public string CourseTitle { get; set; } = null!;
        public string InstructorId { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public string? Description { get; set; }
        public string? MediaUrl { get; set; }
    }
} 