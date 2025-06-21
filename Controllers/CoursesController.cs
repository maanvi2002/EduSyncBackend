using EduSyncProject.Data;
using EduSyncProject.DTO;
using EduSyncProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduSyncProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CoursesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AzureBlobService _blobService;

        public CoursesController(AppDbContext context, AzureBlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }



        [HttpGet]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<ActionResult<IEnumerable<CourseReadDTO>>> GetCourses()
        {
            // Get all courses for both students and instructors
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .ToListAsync();

            var courseDtos = courses.Select(course => new CourseReadDTO
            {
                Id = course.CourseId.ToString(),
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId.ToString(),
                InstructorName = course.Instructor.Name,
                MediaUrl = course.MediaUrl
            });

            return Ok(courseDtos);
        }

       [HttpGet("{id}")]
        [Authorize(Roles = "Instructor,Student")]
        public async Task<ActionResult<CourseReadDTO>> GetCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var courseDto = new CourseReadDTO
            {
                Id = course.CourseId.ToString(),
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId.ToString(),
                InstructorName = course.Instructor.Name,
                MediaUrl = course.MediaUrl
            };

            return Ok(courseDto);
        }



        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PutCourse(Guid id, [FromForm] CourseUpdateWithFileDTO courseDto)
        {
            var course = await _context.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null) return NotFound();

            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (course.InstructorId.ToString() != instructorId)
                return Forbid("You can only edit your own course");

            string? mediaUrl = course.MediaUrl;
            if (courseDto.File != null)
            {
                mediaUrl = await _blobService.UploadAsync(courseDto.File);
            }

            course.Title = courseDto.Title;
            course.Description = courseDto.Description;
            course.MediaUrl = mediaUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }




        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CourseReadDTO>> PostCourse([FromForm] CourseCreateWithFileDTO courseDto)
        {
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(instructorId, out Guid instructorGuid))
                return BadRequest("Invalid instructor ID");

            var instructor = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == instructorGuid && u.Role == "Instructor");

            if (instructor == null)
                return Forbid("Only instructors can create courses");

            string? mediaUrl = null;
            if (courseDto.File != null)
            {
                mediaUrl = await _blobService.UploadAsync(courseDto.File);
            }

            var course = new Course
            {
                Title = courseDto.Title,
                Description = courseDto.Description,
                InstructorId = instructorGuid,
                MediaUrl = mediaUrl
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, new CourseReadDTO
            {
                Id = course.CourseId.ToString(),
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId.ToString(),
                InstructorName = instructor.Name,
                MediaUrl = course.MediaUrl
            });
        }






        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var course = await _context.Courses
                .Include(c => c.Assessments)
                    .ThenInclude(a => a.Results)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            // Verify that the instructor is deleting their own course
            var instructorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (course.InstructorId.ToString() != instructorId)
            {
                return StatusCode(403, new { 
                    message = "Instructors can only delete their own courses",
                    courseInstructorId = course.InstructorId.ToString(),
                    yourInstructorId = instructorId,
                    courseTitle = course.Title
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete all results associated with the course's assessments
                foreach (var assessment in course.Assessments)
                {
                    _context.Results.RemoveRange(assessment.Results);
                }

                // Delete all assessments
                _context.Assessments.RemoveRange(course.Assessments);

                // Delete the course
                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
