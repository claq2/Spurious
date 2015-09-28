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
    [RoutePrefix("api/Subdivision")]
    public class SubdivisionController : ApiController
    {
        // GET: api/Subdivision
        [Route("")]
        public async Task<IEnumerable<Subdivision>> Get()
        {
            return await new SubdivisionService().SubdivisionsAndVolumes();
        }

        // GET: api/Subdivision/5
        [Route("{id}")]
        public async Task<Subdivision> Get(int id)
        {
            return await new SubdivisionService().LoadById(id);
        }

        [Route("top10")]
        public async Task<IEnumerable<Subdivision>> GetTop10()
        {
            return await new SubdivisionService().Top10AlcoholDensity();
        }

        [Route("{id}/boundary")]
        public async Task<object> GetBoundary(int id)
        {
            return await new SubdivisionService().BoundaryGeoJson(id);
        }
    }
}
