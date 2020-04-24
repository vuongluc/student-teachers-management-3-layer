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
using ProjectDomain;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class ClassTypesController : ApiController
    {
        IClassTypeBusiness bizClass = new ClassTypeEF();
        // GET: api/ClassTypes
        public IHttpActionResult GetClassTypes()
        {
            //System.Threading.Thread.Sleep(3000);
            return Ok(bizClass.findAllClassType());
        }

        // GET: api/ClassTypes/5
        [ResponseType(typeof(ClassTypesDTO))]
        public IHttpActionResult GetClassType(string id)
        {
            List<ClassTypesDTO> classType = bizClass.search(id);
            if (classType == null)
            {
                return NotFound();
            }

            return Ok(classType);
        }

        // PUT: api/ClassTypes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutClassType(string id, ClassTypesDTO classType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != classType.TypeId)
            {
                return BadRequest();
            }

            try
            {
                bizClass.updateClass(classType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassTypeExists(id))
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

        // POST: api/ClassTypes
        [ResponseType(typeof(ClassTypesDTO))]
        public IHttpActionResult PostClassType(ClassTypesDTO classType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           
            try
            {
                bizClass.createClass(classType);
            }
            catch (DbUpdateException)
            {
                if (ClassTypeExists(classType.TypeId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = classType.TypeId }, classType);
        }

        // DELETE: api/ClassTypes/5
        [ResponseType(typeof(ClassTypesDTO))]
        public IHttpActionResult DeleteClassType(string id)
        {
            ClassTypesDTO classType = bizClass.findById(id);
            if (classType == null)
            {
                return NotFound();
            }

            bizClass.deleteClass(id);

            return Ok(classType);
        }

        private bool ClassTypeExists(string id)
        {
            return bizClass.findAllClassType().Count(e => e.TypeId == id) > 0;
        }
    }
}