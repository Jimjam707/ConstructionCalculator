using ConstructionCalculator.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace ConstructionCalculator.Tests
{
    [TestFixture]
    public class ConcreteCalculatorTests
    {
        private HomeController _controller;

        [SetUp]
        public void Setup()
        {
            // Mock ILogger to satisfy constructor
            var loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(loggerMock.Object);
        }
        
        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();   // This satisfies the analyzer
        }

        [Test]
        public void ConcreteCalculator_Imperial_Slab_20x15x6_80lb_NoExtra_NoPrice()
        {
            // Act
            var result = _controller.Concrete(
                length: 20m,
                width: 15m,
                depth: 6m,
                pricePerUnit: null,
                unitSystem: "imperial",
                isUnitChange: "false",
                bagSize: "80",
                extraVolume: ""
            ) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null, "Result should be a ViewResult");

            // Access ViewBag via ViewData (dynamic)
            dynamic viewBag = result.ViewData;

            Assert.That(result.ViewData.ModelState.IsValid, Is.True, "ModelState should be valid");
            Assert.That((string)viewBag.VolumeDisplay, Is.EqualTo("150.00 ft³"));
            Assert.That((string)viewBag.BagsNeededDisplay, Is.EqualTo("250 × 80 lb bags"));
            Assert.That(viewBag.CostDisplay, Is.Null);
            Assert.That(viewBag.Result, Is.True);
        }

        [Test]
        public void ConcreteCalculator_Imperial_Slab_20x15x6_80lb_Extra0_5()
        {
            var result = _controller.Concrete(
                length: 20m,
                width: 15m,
                depth: 6m,
                pricePerUnit: null,
                unitSystem: "imperial",
                isUnitChange: "false",
                bagSize: "80",
                extraVolume: "0.5"
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);

            dynamic viewBag = result.ViewData;

            Assert.That((string)viewBag.VolumeDisplay, Is.EqualTo("150.00 ft³"));
            Assert.That((string)viewBag.BagsNeededDisplay, Is.EqualTo("251 × 80 lb bags"));
        }

        [Test]
        public void ConcreteCalculator_Metric_6x4_5x15cm_60lb_Extra1_Price200()
        {
            var result = _controller.Concrete(
                length: 6m,
                width: 4.5m,
                depth: 15m,
                pricePerUnit: 200m,
                unitSystem: "metric",
                isUnitChange: "false",
                bagSize: "60",
                extraVolume: "1"
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);

            dynamic viewBag = result.ViewData;

            Assert.That((string)viewBag.VolumeDisplay, Is.EqualTo("4.05 m³"));
            Assert.That((string)viewBag.BagsNeededDisplay, Is.EqualTo("298 × 60 lb bags"));
            Assert.That((string)viewBag.CostDisplay, Is.EqualTo("$1,010.00"));
        }

        [Test]
        public void ConcreteCalculator_InvalidInput_ReturnsError()
        {
            var result = _controller.Concrete(
                length: 0m,
                width: 10m,
                depth: 6m,
                pricePerUnit: null,
                unitSystem: "imperial",
                isUnitChange: "false",
                bagSize: "80",
                extraVolume: ""
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);

            dynamic viewBag = result.ViewData;

            Assert.That((string)viewBag.Error, Is.EqualTo("All dimensions must be positive numbers."));
            Assert.That(viewBag.LengthError, Is.True);
            Assert.That(viewBag.Result, Is.Null);
        }
    }
}