using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpuriousApi.Models;

namespace SpuriousApi.FunctionalTests
{
    [TestFixture]
    public class LcboServiceTests
    {
        [Test]
        public async void GetStoresTest()
        {
            var service = new LcboService();
            var stores = await service.GetLcboStores();
            Assert.That(stores.Count, Is.EqualTo(100));
            Assert.That(stores.All(s => s.Id > 0));
            Assert.That(stores.All(s => !string.IsNullOrWhiteSpace(s.Name)));
            Assert.That(stores.All(s => !string.IsNullOrWhiteSpace(s.GeoJSON)));
        }
    }
}
