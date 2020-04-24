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
    public class StatusController : ApiController
    {
        IStatusBusiness bizStatus = new StatusEF();

        // GET: api/Status
        public IHttpActionResult GetStatus()
        {
            return Ok(bizStatus.fidAllStatus());
        }

        // GET: api/Status/5
        [ResponseType(typeof(StatusDTO))]
        public IHttpActionResult GetStatus(string id)
        {
            List<StatusDTO> status = bizStatus.search(id);
            if (status == null)
            {
                return NotFound();
            }

            return Ok(status);
        }

        // PUT: api/Status/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutStatus(string id, StatusDTO status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != status.StatusId)
            {
                return BadRequest();
            }

            
            try
            {
                bizStatus.updateStatus(status);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatusExists(id))
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

        // POST: api/Status
        [ResponseType(typeof(StatusDTO))]
        public IHttpActionResult PostStatus(StatusDTO status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            

            try
            {
                bizStatus.createStatus(status);
            }
            catch (DbUpdateException)
            {
                if (StatusExists(status.StatusId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = status.StatusId }, status);
        }

        // DELETE: api/Status/5
        [ResponseType(typeof(StatusDTO))]
        public IHttpActionResult DeleteStatus(string id)
        {
            StatusDTO status = bizStatus.findById(id);
            if (status == null)
            {
                return NotFound();
            }

            bizStatus.deleteStatus(id);

            return Ok(status);
        }

        
        private bool StatusExists(string id)
        {
            return bizStatus.fidAllStatus().Count(e => e.StatusId == id) > 0;
        }
    }
}