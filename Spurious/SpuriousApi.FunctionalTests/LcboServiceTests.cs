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
            var resp = await service.GetLcboStores();
            Assert.That(resp.Contains("Queen's"));
        }
    }
}
