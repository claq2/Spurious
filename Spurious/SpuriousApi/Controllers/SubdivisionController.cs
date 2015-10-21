using SpuriousApi.Models;
using SpuriousApi.ViewModels;
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
            return await new SubdivisionService().Density(AlcoholType.All, EndOfDistribution.Top, 10000);
        }

        // GET: api/Subdivision/5
        [Route("{id}")]
        public async Task<Subdivision> Get(int id)
        {
            return await new SubdivisionService().LoadById(id);
        }

        [Route("top10x")]
        public async Task<ListAndMapView> GetTop10x()
        {
            var n = nameof(Subdivision.OverallAlcoholDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.All, EndOfDistribution.Top, 10);
            return new ListAndMapView
            {
                Title = "Top 10 Overall",
                Subdivisions = subdivs,
                DensityName = "Alcohol",
                DensityPropertyToUse = n
            };
        }

        [Route("top10winex")]
        public async Task<ListAndMapView> GetTop10Winex()
        {
            var n = nameof(Subdivision.WineDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.Wine, EndOfDistribution.Top, 10);
            return new ListAndMapView
            {
                Title = "Top 10 Wine",
                Subdivisions = subdivs,
                DensityName = "Wine",
                DensityPropertyToUse = n
            };
        }

        [Route("top10beerx")]
        public async Task<ListAndMapView> GetTop10Beerx()
        {
            var n = nameof(Subdivision.BeerDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.Beer, EndOfDistribution.Top, 10);
            return new ListAndMapView
            {
                Title = "Top 10 Beer",
                Subdivisions = subdivs,
                DensityName = "Beer",
                DensityPropertyToUse = n
            };
        }

        [Route("top10spiritsx")]
        public async Task<ListAndMapView> GetTop10Spiritsx()
        {
            var n = nameof(Subdivision.WineDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.Spirits, EndOfDistribution.Top, 10);
            return new ListAndMapView
            {
                Title = "Top 10 Spirits",
                Subdivisions = subdivs,
                DensityName = "Spirits",
                DensityPropertyToUse = n
            };
        }

        [Route("allx")]
        public async Task<ListAndMapView> GetAllx()
        {
            var n = nameof(Subdivision.OverallAlcoholDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.All, EndOfDistribution.Top, 100000);
            return new ListAndMapView
            {
                Title = "All",
                Subdivisions = subdivs,
                DensityName = "All",
                DensityPropertyToUse = n
            };
        }

        [Route("bottom10x")]
        public async Task<ListAndMapView> GetBottom10x()
        {
            var n = nameof(Subdivision.OverallAlcoholDensity);
            n = Char.ToLowerInvariant(n[0]) + n.Substring(1);
            var subdivs = await new SubdivisionService().Density(AlcoholType.All, EndOfDistribution.Bottom, 10);
            return new ListAndMapView
            {
                Title = "Bottom 10 Overall",
                Subdivisions = subdivs,
                DensityName = "Alcohol",
                DensityPropertyToUse = n
            };
        }

        [Route("bottom10")]
        public async Task<IEnumerable<Subdivision>> GetBottom10Spirits()
        {
            return await new SubdivisionService().Density(AlcoholType.All, EndOfDistribution.Bottom, 10);
        }

        [Route("{id}/boundary")]
        public async Task<object> GetBoundary(int id)
        {
            return await new SubdivisionService().BoundaryGeoJson(id);
        }
    }
}
