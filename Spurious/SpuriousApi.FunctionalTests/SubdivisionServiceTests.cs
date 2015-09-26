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
    public class SubdivisionServiceTests
    {
        [Test]
        public async void Load100Succeeds()
        {
            var service = new SubdivisionService();
            var subdivs = await service.Load100();
            Assert.That(subdivs.Count(), Is.EqualTo(100));
            Assert.That(subdivs.All(s => s.Id > 0));
            Assert.That(subdivs.All(s => s.Population.HasValue));
            Assert.That(subdivs.All(s => !string.IsNullOrWhiteSpace(s.GeoJSON)));
        }

        [Test]
        public async void LoadByIdSucceeds()
        {
            var service = new SubdivisionService();
            var subdiv = await service.LoadById(1001101);
            Assert.That(subdiv.Id, Is.GreaterThan(0));
            Assert.That(subdiv.Population, Is.Not.Null);
            Assert.That(subdiv.GeoJSON, Is.Not.Null);
        }

        [Test]
        public async void LoadSubdivsAndVolumes()
        {
            var service = new SubdivisionService();
            var subdivs = await service.SubdivisionsAndVolumes();
            Assert.That(subdivs.All(s => s.Id > 0));
            Assert.That(subdivs.All(s => s.Population.HasValue));
            Assert.That(subdivs.All(s => !string.IsNullOrWhiteSpace(s.GeoJSON)));
            foreach (var subdiv in subdivs)
            {
                Assert.That(subdiv.Volumes.Total, Is.GreaterThanOrEqualTo(0), $"subdiv {subdiv.Id} has total {subdiv.Volumes.Total}");
            }
        }

        [Test]
        public async void LoadTop10Densities()
        {
            var service = new SubdivisionService();
            var subdivs = await service.Top10AlcoholDensity();
            Assert.That(subdivs.Count, Is.EqualTo(10));
            Assert.That(subdivs.All(s => s.Id > 0));
            Assert.That(subdivs.All(s => s.Population.HasValue));
            Assert.That(subdivs.All(s => !string.IsNullOrWhiteSpace(s.GeoJSON)));
            Assert.That(subdivs.All(s => s.LcboStores.Count > 0));
            Assert.That(subdivs.All(s => s.CentreLatitude != 0.0));
            Assert.That(subdivs.All(s => s.CentreLongitude != 0.0));
            foreach (var subdiv in subdivs)
            {
                Assert.That(subdiv.Volumes.Total, Is.GreaterThan(0), $"subdiv {subdiv.Id} has total {subdiv.Volumes.Total}");
            }
        }
    }
}
