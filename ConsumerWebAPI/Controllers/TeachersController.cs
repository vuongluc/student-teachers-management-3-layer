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
    public class TeachersController : ApiController
    {
        ITeacherBusiness bizTeacher = new TeacherEF();
        // GET: api/Teachers
        public IHttpActionResult GetTeachers()
        {
            return Ok(bizTeacher.findAllTeacher());
        }

        // GET: api/Teachers/5
        [ResponseType(typeof(List<TeacherDTO>))]
        public IHttpActionResult GetTeacher(string id)
        {
            List<TeacherDTO> teacher = bizTeacher.search(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return Ok(teacher);
        }

        // PUT: api/Teachers/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutTeacher(string id, TeacherDTO teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != teacher.TeacherId)
            {
                return BadRequest();
            }

            
            try
            {
                bizTeacher.updateTeacher(teacher);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
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

        // POST: api/Teachers
        [ResponseType(typeof(TeacherDTO))]
        public IHttpActionResult PostTeacher(TeacherDTO teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            

            try
            {
                bizTeacher.createTeacher(teacher);
            }
            catch (DbUpdateException)
            {
                if (TeacherExists(teacher.TeacherId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = teacher.TeacherId }, teacher);
        }

        // DELETE: api/Teachers/5
        [ResponseType(typeof(TeacherDTO))]
        public IHttpActionResult DeleteTeacher(string id)
        {
            TeacherDTO teacher = bizTeacher.findById(id);
            if (teacher == null)
            {
                return NotFound();
            }

            bizTeacher.deleteTeacher(id);

            return Ok(teacher);
        }

       

        private bool TeacherExists(string id)
        {
            return bizTeacher.findAllTeacher().Count(e => e.TeacherId == id) > 0;
        }
    }
}