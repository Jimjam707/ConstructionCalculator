using Microsoft.AspNetCore.Mvc;
using System;

namespace ConstructionCalculator.Controllers
{
    public class HomeController : Controller
    {
        
        public HomeController()
        {
            
        }
        
        
        [HttpGet]
        public IActionResult Concrete()
        {
            ViewBag.UnitSystem = "imperial";
            return View();
        }

        [HttpPost]
        public IActionResult Concrete(
            decimal? length,
            decimal? width,
            decimal? depth,
            decimal? pricePerUnit,
            string unitSystem = "imperial",
            string isUnitChange = "false",
            string bagSize = "80",
            string extraVolume = "")
        {
            ViewBag.UnitSystem = unitSystem;
            ViewData["length"] = length;
            ViewData["width"] = width;
            ViewData["depth"] = depth;
            ViewData["pricePerUnit"] = pricePerUnit;
            ViewData["bagSize"] = bagSize;
            ViewData["extraVolume"] = extraVolume;

            
            ViewBag.LengthError = ViewBag.WidthError = ViewBag.DepthError = null;
            ViewBag.Error = null;
            ViewBag.Result = null;

            // If only switching units
            if (isUnitChange == "true")
            {
                return View();
            }

           
            if (!length.HasValue || !width.HasValue || !depth.HasValue ||
                length <= 0 || width <= 0 || depth <= 0)
            {
                ViewBag.Error = "All dimensions must be positive numbers.";
                if (!length.HasValue || length <= 0) ViewBag.LengthError = true;
                if (!width.HasValue || width <= 0) ViewBag.WidthError = true;
                if (!depth.HasValue || depth <= 0) ViewBag.DepthError = true;
                return View();
            }

            // Calculation variables (initialized to avoid unassigned errors)
            decimal volumeBase = 0m;
            decimal volumeDisplay = 0m;
            string volumeUnit = "";
            decimal bagVolume = 0m;

            // Calculate volume and set bag volume (same unit)
            if (unitSystem == "metric")
            {
                decimal depthM = depth.Value / 100m;
                decimal volumeM3 = length.Value * width.Value * depthM;

                volumeBase = volumeM3;
                volumeDisplay = volumeM3;
                volumeUnit = "m³";

                // Bag volume in m³
                if (bagSize == "40")      bagVolume = 0.011m;
                else if (bagSize == "60") bagVolume = 0.017m;
                else                      bagVolume = 0.036m; // 80 lb
            }
            else // imperial
            {
                decimal depthFt = depth.Value / 12m;
                decimal volumeCuFt = length.Value * width.Value * depthFt;

                volumeBase = volumeCuFt / 27m;  // yd³
                volumeDisplay = volumeCuFt;     // display ft³
                volumeUnit = "ft³";

                // Bag volume in yd³ (exact fractions)
                if (bagSize == "40")      bagVolume = 0.3m / 27m;   // ≈ 0.0111 yd³
                else if (bagSize == "60") bagVolume = 0.45m / 27m;  // ≈ 0.0167 yd³
                else                      bagVolume = 0.6m / 27m;   // ≈ 0.0222 yd³
            }
            
            decimal extra = 0m;
            if (!string.IsNullOrWhiteSpace(extraVolume))
            {
                if (decimal.TryParse(extraVolume, out decimal parsed) && parsed >= 0)
                {
                    extra = parsed;
                }
            }

            decimal volumeWithExtra = volumeBase + extra;

            // Bags needed
            decimal bagsExact = volumeWithExtra / bagVolume;
            decimal bagsRounded = Math.Round(bagsExact, 6, MidpointRounding.AwayFromZero);
            int bagsNeeded = (int)Math.Ceiling(bagsRounded);
            
            
            ViewBag.VolumeDisplay = $"{volumeDisplay:F2} {volumeUnit}";
            ViewBag.BagsNeededDisplay = $"{bagsNeeded} × {bagSize} lb bags";

            if (extra > 0)
            {
                ViewBag.ExtraDisplay = $"{extra:F2} {(unitSystem == "imperial" ? "yd³" : "m³")}";
            }

            if (pricePerUnit.HasValue && pricePerUnit > 0)
            {
                decimal cost = volumeWithExtra * pricePerUnit.Value;
                ViewBag.CostDisplay = $"{cost:C2}";
            }

            ViewBag.Result = true;

            return View();
        }
        
        [HttpGet]
        public IActionResult Aggregate()
        {
            ViewBag.UnitSystem = "imperial"; 
            return View();
        }

        [HttpPost]
        public IActionResult Aggregate(
            decimal? length,
            decimal? width,
            decimal? depth,
            string unitSystem = "imperial",
            string isUnitChange = "false",
            string material = "crushed stone", 
            string extraVolume = "")
        {
            ViewBag.UnitSystem = unitSystem;
            ViewData["length"] = length;
            ViewData["width"] = width;
            ViewData["depth"] = depth;
            ViewData["material"] = material;
            ViewData["extraVolume"] = extraVolume;

            ViewBag.LengthError = ViewBag.WidthError = ViewBag.DepthError = null;
            ViewBag.Error = null;
            ViewBag.Result = null;

            if (isUnitChange == "true")
            {
                return View();
            }

            if (!length.HasValue || !width.HasValue || !depth.HasValue ||
                length <= 0 || width <= 0 || depth <= 0)
            {
                ViewBag.Error = "All dimensions must be positive numbers.";
                if (!length.HasValue || length <= 0) ViewBag.LengthError = true;
                if (!width.HasValue || width <= 0) ViewBag.WidthError = true;
                if (!depth.HasValue || depth <= 0) ViewBag.DepthError = true;
                return View();
            }

            // Calculate volume
            decimal volumeBase;
            decimal volumeDisplay;
            string volumeUnit;

            if (unitSystem == "metric")
            {
                decimal depthM = depth.Value / 100m;
                decimal volumeM3 = length.Value * width.Value * depthM;
                volumeBase = volumeM3;
                volumeDisplay = volumeM3;
                volumeUnit = "m³";
            }
            else
            {
                decimal depthFt = depth.Value / 12m;
                decimal volumeCuFt = length.Value * width.Value * depthFt;
                volumeBase = volumeCuFt / 27m; // yd³
                volumeDisplay = volumeCuFt;
                volumeUnit = "ft³";
            }

            // Material density  
            decimal densityTonsPerYd3;
            string materialName;

            switch (material.ToLower())
            {
                case "crushed stone":
                    densityTonsPerYd3 = 1.6m; 
                    materialName = "Crushed Stone";
                    break;
                case "gravel":
                    densityTonsPerYd3 = 1.4m;
                    materialName = "Gravel";
                    break;
                case "sand":
                    densityTonsPerYd3 = 1.2m;
                    materialName = "Sand";
                    break;
                default:
                    densityTonsPerYd3 = 1.5m; 
                    materialName = material;
                    break;
            }

    
            decimal extra = 0m;
            if (!string.IsNullOrWhiteSpace(extraVolume))
            {
                if (decimal.TryParse(extraVolume, out decimal parsed) && parsed >= 0)
                {
                    extra = parsed;
                }
            }

            decimal volumeWithExtra = volumeBase + extra;

            // Tons needed
            decimal tonsNeeded = volumeWithExtra * densityTonsPerYd3;

            
            ViewBag.VolumeDisplay = $"{volumeDisplay:F2} {volumeUnit}";
            ViewBag.TonsDisplay = $"{tonsNeeded:F2} tons";
            ViewBag.MaterialDisplay = materialName;

            if (extra > 0)
            {
                ViewBag.ExtraDisplay = $"+ {extra:F2} {(unitSystem == "imperial" ? "yd³" : "m³")}";
            }

            ViewBag.Result = true;

            return View();
        }
            
        
    }
}