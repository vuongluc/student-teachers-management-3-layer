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
using ProjectDomain.Business.Capable;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class CapablesController : ApiController
    {
        ICapableBusiness bizCap = new CapableEF();

        // GET: api/Capables
        public IHttpActionResult GetCapables()
        {
            return Ok(bizCap.findAllCapable());

        }

        // GET: api/Capables/5
        [ResponseType(typeof(List<CapableDTO>))]
        public IHttpActionResult GetCapable(string id)
        {
            List<CapableDTO> capable = bizCap.findById(id);
            if (capable == null)
            {
                return NotFound();
            }

            return Ok(capable);
        }

        // PUT: api/Capables/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutCapable(string id, CapableDTO capable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != capable.TeacherId)
            {
                return BadRequest();
            }
            
            try
            {
                bizCap.updateCapable(capable);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CapableExists(id))
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

        // POST: api/Capables
        [ResponseType(typeof(CapableDTO))]
        public IHttpActionResult PostCapable(CapableDTO capable)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bizCap.createCapable(capable);

            return CreatedAtRoute("DefaultApi", new { id = capable.id }, capable);
        }

        // DELETE: api/Capables/5
        [ResponseType(typeof(CapableDTO))]
        public IHttpActionResult DeleteCapable(string id)
        {
            List<CapableDTO> capable = bizCap.findById(id);
            if (capable == null)
            {
                return NotFound();
            }
            bizCap.deleteCapable(id);
           
            return Ok(capable);
        }

       

        private bool CapableExists(string id)
        {
            return bizCap.findAllCapable().Count(e => e.TeacherId == id) > 0;
        }
    }
}