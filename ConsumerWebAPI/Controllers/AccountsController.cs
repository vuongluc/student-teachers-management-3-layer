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
using ProjectDomain.Business.Login;
using ProjectDomain.DTOs;

namespace ConsumerWebAPI.Controllers
{
    public class AccountsController : ApiController
    {
        ILoginBusiness bizAccount = new LoginEF();

        // GET: api/Accounts
        public IHttpActionResult GetAccounts()
        {
            return Ok(bizAccount.findAllAccount());
        }

        [Route("api/Accounts/{id}/{name}")]
        public IEnumerable<AccountDTO> GetAccounts(string id, string name)
        {
            return bizAccount.findAllAccount();
        }

        // GET: api/Accounts/5
        [ResponseType(typeof(AccountDTO))]
        public IHttpActionResult GetAccount(string id)
        {
            AccountDTO account = bizAccount.findById(id);
            if (account == null)
            {
                return NotFound();
            }

            return Ok(account);
        }

        // PUT: api/Accounts/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutAccount(string id, AccountDTO account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != account.username)
            {
                return BadRequest();
            }

            
            try
            {
                bizAccount.updateAccount(account);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(id))
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

        // POST: api/Accounts
        [ResponseType(typeof(AccountDTO))]
        public IHttpActionResult PostAccount(AccountDTO account)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                bizAccount.createAccount(account);
            }
            catch (DbUpdateException)
            {
                if (AccountExists(account.username))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = account.username }, account);
        }

        // DELETE: api/Accounts/5
        [ResponseType(typeof(Account))]
        public IHttpActionResult DeleteAccount(string id)
        {
            AccountDTO account = bizAccount.findById(id);
            if (account == null)
            {
                return NotFound();
            }                    
            return Ok(account);
        }

       

        private bool AccountExists(string id)
        {
            return bizAccount.findAllAccount().Count(e => e.username == id) > 0;
        }
    }
}