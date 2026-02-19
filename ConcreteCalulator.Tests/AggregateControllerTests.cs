using ConstructionCalculator.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace ConcreteCalcTests 
{
    [TestFixture]
    public class AggregateControllerTests
    {
        private HomeController controller;

        [SetUp]
        public void Setup()
        {
            controller = new HomeController();
        }

        [TearDown]
        public void TearDown()
        {
            controller?.Dispose();
        }

        [Test]
        public void Aggregate_Post_ValidImperial_CrushedStone_NoExtra()
        {
            var result = controller.Aggregate(
                length: 20m,
                width: 15m,
                depth: 6m,
                unitSystem: "imperial",
                isUnitChange: "false",
                material: "crushed stone",
                extraVolume: ""
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewData.ModelState.IsValid, Is.True);

            dynamic vb = controller.ViewBag;
            Assert.That((string)vb.VolumeDisplay, Is.EqualTo("150.00 ft³"));
            Assert.That((string)vb.TonsDisplay, Is.EqualTo("8.89 tons"));
            Assert.That(vb.Result, Is.True);
        }

        [Test]
        public void Aggregate_Post_ValidMetric_Gravel_WithExtra()
        {
            var result = controller.Aggregate(
                length: 6m,
                width: 4.5m,
                depth: 15m,
                unitSystem: "metric",
                isUnitChange: "false",
                material: "gravel",
                extraVolume: "1"
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);

            dynamic vb = controller.ViewBag;
            Assert.That((string)vb.VolumeDisplay, Is.EqualTo("4.05 m³"));
            Assert.That((string)vb.TonsDisplay, Is.EqualTo("7.07 tons"));  
            Assert.That((string)vb.ExtraDisplay, Is.EqualTo("+ 1.00 m³"));
            Assert.That(vb.Result, Is.True);
        }

        [Test]
        public void Aggregate_Post_InvalidInput_ShowsError()
        {
            var result = controller.Aggregate(
                length: 0m,
                width: 10m,
                depth: 6m,
                unitSystem: "imperial",
                isUnitChange: "false",
                material: "sand",
                extraVolume: ""
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);

            dynamic vb = controller.ViewBag;
            Assert.That((string)vb.Error, Is.EqualTo("All dimensions must be positive numbers."));
            Assert.That(vb.LengthError, Is.True);
            Assert.That(vb.Result, Is.Null);
        }
    }
}