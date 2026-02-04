using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ConstructionCalculator.Models;

namespace ConstructionCalculator.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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
    string extraVolume = "")   // empty string = no extra
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

    decimal volumeBase;
    decimal volumeDisplay;
    string volumeUnit;
    decimal bagVolume;

    if (unitSystem == "metric")
    {
        decimal depthM = depth.Value / 100m;
        volumeBase = length.Value * width.Value * depthM;
        volumeDisplay = volumeBase;
        volumeUnit = "m³";

        if (bagSize == "40")      bagVolume = 0.011m;
        else if (bagSize == "60") bagVolume = 0.017m;
        else                      bagVolume = 0.036m;
    }
    else
    {
        decimal depthFt = depth.Value / 12m;
        decimal volumeCuFt = length.Value * width.Value * depthFt;
        volumeBase = volumeCuFt / 27m;
        volumeDisplay = volumeCuFt;
        volumeUnit = "ft³";

        if (bagSize == "40")      bagVolume = 0.30m;
        else if (bagSize == "60") bagVolume = 0.45m;
        else                      bagVolume = 0.60m;
    }

    // Parse optional extra volume (default 0 if blank or invalid)
    decimal extra = 0m;
    if (!string.IsNullOrWhiteSpace(extraVolume) && decimal.TryParse(extraVolume, out decimal parsedExtra) && parsedExtra >= 0)
    {
        extra = parsedExtra;
    }

    decimal volumeWithExtra = volumeBase + extra;

    int bagsNeeded = (int)Math.Ceiling(volumeWithExtra / bagVolume);

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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}