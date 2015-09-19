using SpuriousApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SpuriousApi.Controllers
{
    public class SubdivisionController : ApiController
    {
        // GET: api/Subdivision
        public async Task<IEnumerable<Subdivision>> Get()
        {
            return await new SubdivisionService().Load100();
        }

        // GET: api/Subdivision/5
        public async Task<Subdivision> Get(int id)
        {
            return await new SubdivisionService().LoadById(id);
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
