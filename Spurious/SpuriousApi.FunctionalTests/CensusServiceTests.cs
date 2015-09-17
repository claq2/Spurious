﻿using System;
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
        public async void Load100Succeeds()
        {
            var service = new CensusService();
            var subdivs = await service.Load100();
            Assert.That(subdivs.Count(), Is.EqualTo(100));
            Assert.That(subdivs.All(s => s.Id > 0));
            Assert.That(subdivs.All(s => s.Population.HasValue));
            Assert.That(subdivs.All(s => !string.IsNullOrWhiteSpace(s.GeoJSON)));
        }

        [Test]
        public async void LoadByIdSucceeds()
        {
            var service = new CensusService();
            var subdiv = await service.LoadById(1001101);
            Assert.That(subdiv.Id, Is.GreaterThan(0));
            Assert.That(subdiv.Population, Is.Not.Null);
            Assert.That(subdiv.GeoJSON, Is.Not.Null);
        }
    }
}
