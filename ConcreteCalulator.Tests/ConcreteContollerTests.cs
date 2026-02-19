using ConstructionCalculator.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace ConcreteCalcTests
{
    [TestFixture]
    public class ConcreteControllerTests
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
        public void Concrete_Post_ValidImperial_ReturnsViewWithCorrectVolume()
        {
            var result = controller.Concrete(
                length: 20m,
                width: 15m,
                depth: 6m,
                pricePerUnit: null,
                unitSystem: "imperial",
                isUnitChange: "false",
                bagSize: "80",
                extraVolume: ""
            ) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewData.ModelState.IsValid, Is.True);

            
            Assert.That((string)controller.ViewBag.VolumeDisplay, Is.EqualTo("150.00 ft³"));
            Assert.That((string)controller.ViewBag.BagsNeededDisplay, Is.EqualTo("250 × 80 lb bags"));
            Assert.That(controller.ViewBag.CostDisplay, Is.Null);
            Assert.That(controller.ViewBag.Result, Is.True);
        }

        [Test]
        public void Concrete_Post_ValidMetric_WithPrice_ReturnsCorrectCost()
        {
            var result = controller.Concrete(
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

            Assert.That((string)controller.ViewBag.VolumeDisplay, Is.EqualTo("4.05 m³"));
            Assert.That((string)controller.ViewBag.BagsNeededDisplay, Is.EqualTo("298 × 60 lb bags"));
            Assert.That((string)controller.ViewBag.CostDisplay, Is.EqualTo("$1,010.00"));
        }

        [Test]
        public void Concrete_Post_ZeroLength_ShowsError()
        {
            var result = controller.Concrete(
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

            Assert.That((string)controller.ViewBag.Error, Is.EqualTo("All dimensions must be positive numbers."));
            Assert.That(controller.ViewBag.LengthError, Is.True);
            Assert.That(controller.ViewBag.Result, Is.Null);
        }
        
        [Test]
        public void Concrete_Post_SmallImperial_10x10x12_80lb_NoExtra_NoPrice()
        {
            var result = controller.Concrete(
                length: 10m,
                width: 10m,
                depth: 12m,
                pricePerUnit: null,
                unitSystem: "imperial",
                isUnitChange: "false",
                bagSize: "80",
                extraVolume: ""
            ) as ViewResult;

            Assert.That(result, Is.Not.Null, "Should return a ViewResult");
            Assert.That(result.ViewData.ModelState.IsValid, Is.True, "ModelState should be valid");

            Assert.That((string)controller.ViewBag.VolumeDisplay, Is.EqualTo("100.00 ft³"));
            Assert.That((string)controller.ViewBag.BagsNeededDisplay, Is.EqualTo("167 × 80 lb bags"));
            Assert.That(controller.ViewBag.CostDisplay, Is.Null);
            Assert.That(controller.ViewBag.Result, Is.True);
        }
    }
}