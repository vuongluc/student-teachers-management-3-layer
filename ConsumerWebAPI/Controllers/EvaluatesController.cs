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
using ProjectDomain.Business.Evaluate;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class EvaluatesController : ApiController
    {
        IEvaluatesBusiness bizEvalue = new EvaluateEF();

        // GET: api/Evaluates
        public IHttpActionResult GetEvaluates()
        {
            return Ok(bizEvalue.findAllEvaluate());
        }

        // GET: api/Evaluates/5
        [ResponseType(typeof(List<EvaluateDTO>))]
        public IHttpActionResult GetEvaluate(string id)
        {
            List<EvaluateDTO> evaluate = bizEvalue.search(id);
            if (evaluate == null)
            {
                return NotFound();
            }

            return Ok(evaluate);
        }

        // PUT: api/Evaluates/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEvaluate(string id, EvaluateDTO evaluate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != evaluate.StudentId + evaluate.ClassId)
            {
                return BadRequest();
            }
            
            try
            {
                bizEvalue.updateEvaluate(evaluate);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EvaluateExists(id))
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

        // POST: api/Evaluates
        [ResponseType(typeof(EvaluateDTO))]
        public IHttpActionResult PostEvaluate(EvaluateDTO evaluate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

           

            try
            {
                bizEvalue.createEvaluate(evaluate);
            }
            catch (DbUpdateException)
            {
                if (EvaluateExists(evaluate.StudentId + evaluate.ClassId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = evaluate.StudentId }, evaluate);
        }

        // DELETE: api/Evaluates/5
        [ResponseType(typeof(Evaluate))]
        public IHttpActionResult DeleteEvaluate(string id)
        {
            EvaluateDTO evaluate = bizEvalue.findById(id);
            if (evaluate == null)
            {
                return NotFound();
            }

            bizEvalue.deleteEvaluate(id);

            return Ok(evaluate);
        }

        

        private bool EvaluateExists(string id)
        {
            return bizEvalue.findAllEvaluate().Count(e => e.StudentId + e.ClassId == id) > 0;
        }
    }
}