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
    public class ModulesController : ApiController
    {
        IModuleBusiness bizModule = new ModuleEF();
        // GET: api/Modules
        public IHttpActionResult GetModules()
        {
            return Ok(bizModule.findAllModule());
        }

        // GET: api/Modules/5
        [ResponseType(typeof(List<ModuleDTO>))]
        public IHttpActionResult GetModule(string id)
        {
            List<ModuleDTO> module = bizModule.search(id);
            if (module == null)
            {
                return NotFound();
            }

            return Ok(module);
        }

        // PUT: api/Modules/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutModule(string id, ModuleDTO module)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != module.ModuleId)
            {
                return BadRequest();
            }

            

            try
            {
                bizModule.updateModule(module);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModuleExists(id))
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

        // POST: api/Modules
        [ResponseType(typeof(ModuleDTO))]
        public IHttpActionResult PostModule(ModuleDTO module)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            

            try
            {
                bizModule.createModule(module);
            }
            catch (DbUpdateException)
            {
                if (ModuleExists(module.ModuleId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = module.ModuleId }, module);
        }

        // DELETE: api/Modules/5
        [ResponseType(typeof(ModuleDTO))]
        public IHttpActionResult DeleteModule(string id)
        {
            ModuleDTO module = bizModule.findById(id);
            if (module == null)
            {
                return NotFound();
            }

            bizModule.deleteModule(id);

            return Ok(module);
        }

        

        private bool ModuleExists(string id)
        {
            return bizModule.findAllModule().Count(e => e.ModuleId == id) > 0;
        }
    }
}