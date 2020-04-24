using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectDomain.EF;
using ProjectDomain.Business;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class StudentsController : ApiController
    {
        IStudentBusiness bizStudent = new StudentEF();
        // GET: api/Students
        [HttpGet]
        public IHttpActionResult GetStudents()
        {
            return Ok(bizStudent.findAllStudent());
        }

        // GET: api/Students/5
        [HttpGet]
        [ResponseType(typeof(List<StudentDTO>))]
        public IHttpActionResult GetStudent(string id)
        {
            List<StudentDTO> student = bizStudent.search(id);
            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }

        //[HttpGet]
        //[ResponseType(typeof(List<string>))]
        //public IHttpActionResult GetStudent(int num)
        //{
        //    List<string> student = bizStudent.listId();
        //    if (student == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(student);
        //}


        // PUT: api/Students/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStudent(string id, StudentDTO student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != student.StudentId)
            {
                return BadRequest();
            }


            try
            {
                bizStudent.updateStudent(student);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Students
        [ResponseType(typeof(StudentDTO))]
        public IHttpActionResult PostStudent(StudentDTO student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                bizStudent.createStudent(student);
            }
            catch (DbUpdateException)
            {
                if (StudentExists(student.StudentId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = student.StudentId }, student);
        }

        // DELETE: api/Students/5
        [ResponseType(typeof(StudentDTO))]
        public IHttpActionResult DeleteStudent(string id)
        {
            StudentDTO student = bizStudent.findById(id);
            if (student == null)
            {
                return NotFound();
            }

            bizStudent.deleteStudent(id);

            return Ok(student);
        }


        private bool StudentExists(string id)
        {
            return bizStudent.findAllStudent().Count(e => e.StudentId == id) > 0;
        }
    }
}