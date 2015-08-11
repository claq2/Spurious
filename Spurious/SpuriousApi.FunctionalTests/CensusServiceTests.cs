using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SpuriousApi.Models;
namespace SpuriousApi.FunctionalTests
{
    [TestFixture]
    public class CensusServiceTests
    {
        [Test]
        public void Load100Succeeds()
        {
            var service = new CensusService();
            var subdivs = service.Load100();
            Assert.That(subdivs.Count(), Is.EqualTo(100));
            var firstSubdiv = subdivs.First();
            Assert.That(firstSubdiv.Id, Is.GreaterThan(0));
            Assert.That(firstSubdiv.Population, Is.Not.Null);
            Assert.That(firstSubdiv.GeoJSON, Is.Not.Null);
        }

        [Test]
        public void LoadByIdSucceeds()
        {
            var service = new CensusService();
            var subdiv = service.LoadById(1001101);
            Assert.That(subdiv.Id, Is.GreaterThan(0));
            Assert.That(subdiv.Population, Is.Not.Null);
            Assert.That(subdiv.GeoJSON, Is.Not.Null);
        }
    }
}
