using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class tbl_ComInitController : ApiController
    {
        private FPWorkEntities db = new FPWorkEntities();

        // GET: api/tbl_ComInit
        public IQueryable<tbl_ComInit> Gettbl_ComInit()
        {
            return db.tbl_ComInit;
        }

        // GET: api/tbl_ComInit/5
        [ResponseType(typeof(tbl_ComInit))]
        public async Task<IHttpActionResult> Gettbl_ComInit(long id)
        {
            tbl_ComInit tbl_ComInit = await db.tbl_ComInit.FindAsync(id);
            if (tbl_ComInit == null)
            {
                return NotFound();
            }

            return Ok(tbl_ComInit);
        }

        // PUT: api/tbl_ComInit/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Puttbl_ComInit(long id, tbl_ComInit tbl_ComInit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tbl_ComInit.id)
            {
                return BadRequest();
            }

            db.Entry(tbl_ComInit).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tbl_ComInitExists(id))
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

        // POST: api/tbl_ComInit
        [ResponseType(typeof(tbl_ComInit))]
        public async Task<IHttpActionResult> Posttbl_ComInit(tbl_ComInit tbl_ComInit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.tbl_ComInit.Add(tbl_ComInit);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = tbl_ComInit.id }, tbl_ComInit);
        }

        // DELETE: api/tbl_ComInit/5
        [ResponseType(typeof(tbl_ComInit))]
        public async Task<IHttpActionResult> Deletetbl_ComInit(long id)
        {
            tbl_ComInit tbl_ComInit = await db.tbl_ComInit.FindAsync(id);
            if (tbl_ComInit == null)
            {
                return NotFound();
            }

            db.tbl_ComInit.Remove(tbl_ComInit);
            await db.SaveChangesAsync();

            return Ok(tbl_ComInit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool tbl_ComInitExists(long id)
        {
            return db.tbl_ComInit.Count(e => e.id == id) > 0;
        }
    }
}