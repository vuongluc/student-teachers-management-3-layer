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
using ProjectDomain.Business.Enroll;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class EnrollsController : ApiController
    {
       
        IEnrollBusiness bizEnroll = new EnrollEF();

        // GET: api/Enrolls
        public IHttpActionResult GetEnrolls()
        {
            return Ok(bizEnroll.findAllEnroll());
        }

        // GET: api/Enrolls/5
        [ResponseType(typeof(List<EnrollDTO>))]
        public IHttpActionResult GetEnroll(string id)
        {
            List<EnrollDTO> enroll = bizEnroll.search(id);
            if (enroll == null)
            {
                return NotFound();
            }

            return Ok(enroll);
        }

        // PUT: api/Enrolls/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEnroll(string query, EnrollDTO enroll)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (query != enroll.StudentId + enroll.ClassId)
            {
                return BadRequest();
            }            

            try
            {
                bizEnroll.updateEnroll(enroll);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollExists(query))
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

        // POST: api/Enrolls
        [ResponseType(typeof(EnrollDTO))]
        public IHttpActionResult PostEnroll(EnrollDTO enroll)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                bizEnroll.createEnroll(enroll);
            }
            catch (DbUpdateException)
            {
                if (EnrollExists(enroll.StudentId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = enroll.StudentId }, enroll);
        }

        // DELETE: api/Enrolls/5
        [ResponseType(typeof(EnrollDTO))]
        public IHttpActionResult DeleteEnroll(string id)
        {
            EnrollDTO enroll = bizEnroll.findById(id);
            if (enroll == null)
            {
                return NotFound();
            }

            bizEnroll.deleteEnroll(id);

            return Ok(enroll);
        }

       

        private bool EnrollExists(string id)
        {
            return bizEnroll.findAllEnroll().Count(e => e.StudentId + e.ClassId == id) > 0;
        }
    }
}