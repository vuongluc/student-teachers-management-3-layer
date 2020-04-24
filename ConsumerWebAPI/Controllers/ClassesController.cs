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
using ProjectDomain.Business.Class;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class ClassesController : ApiController
    {
        IClassBusiness bizClass = new ClassEF();
        // GET: api/Classes
        public IHttpActionResult GetClasses()
        {
            return Ok(bizClass.findAllClass());
        }

        // GET: api/Classes/5
        [ResponseType(typeof(List<ClassDTO>))]
        public IHttpActionResult GetClass(string id)
        {
            List<ClassDTO> @class = bizClass.search(id);
            if (@class == null)
            {
                return NotFound();
            }

            return Ok(@class);
        }

        // PUT: api/Classes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutClass(string id, ClassDTO @class)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != @class.ClassId)
            {
                return BadRequest();
            }

           
            try
            {
                bizClass.updateClass(@class);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassExists(id))
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

        // POST: api/Classes
        [ResponseType(typeof(Class))]
        public IHttpActionResult PostClass(ClassDTO @class)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                bizClass.createClass(@class);
            }
            catch (DbUpdateException)
            {
                if (ClassExists(@class.ClassId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = @class.ClassId }, @class);
        }

        // DELETE: api/Classes/5
        [ResponseType(typeof(ClassDTO))]
        public IHttpActionResult DeleteClass(string id)
        {
            ClassDTO @class = bizClass.findById(id);
            if (@class == null)
            {
                return NotFound();
            }
            bizClass.deleteClass(id);
            return Ok(@class);
        }

       

        private bool ClassExists(string id)
        {
            return bizClass.findAllClass().Count(e => e.ClassId == id) > 0;
        }
    }
}