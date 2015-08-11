using SpuriousApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SpuriousApi.Controllers
{
    public class SubdivisionController : ApiController
    {
        // GET: api/Subdivision
        public IEnumerable<Subdivision> Get()
        {
            return new CensusService().Load100();
        }

        // GET: api/Subdivision/5
        public Subdivision Get(int id)
        {
            return new CensusService().LoadById(id);
        }

        // POST: api/Subdivision
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Subdivision/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Subdivision/5
        public void Delete(int id)
        {
        }
    }
}
