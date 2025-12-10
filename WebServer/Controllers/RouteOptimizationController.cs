using Microsoft.AspNetCore.Mvc;
using RouteOptimizer.Algorithms;
using RouteOptimizer.Algorithms.AntColony;
using RouteOptimizer.Algorithms.Inputs;
using RouteOptimizer.Exceptions;
using RouteOptimizer.Models;
using RouteOptimizer.Validators;
using WebServer.Models;

namespace WebServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RouteOptimizationController : ControllerBase
    {
        private readonly ILogger<RouteOptimizationController> _logger;

        public RouteOptimizationController(ILogger<RouteOptimizationController> logger)
        {
            _logger = logger;
        }

        [HttpPost("ant-colony-uav-routes")]
        public async Task<IActionResult> GetUavAntColonyRoutes(UavRouteAntAlgorithmInputModel input)
        {
            try
            {
                var optimizer = new RouteOptimizer.RouteOptimizer<UavRouteAntAlgorithmInput, UavRouteAlgorithmOutput>(
                                                              new SmoothedAntColonyUavAlgorithm(
                                                                  new UavRouteAntValidator()));

                var mappedInput = new UavRouteAntAlgorithmInput
                {
                    Uavs = input.Uavs.Select(WebMapper.UavModelToUav),
                    Targets = input.Targets.Select(WebMapper.PointModelToPoint).ToArray()
                };

                var results = optimizer.OptimizeRoutes(mappedInput);

                var outputResults = results.Routes.Select(x => new RouteOutputModel
                {
                    Uav = WebMapper.UavToUavModel(x.Uav),
                    Segments = x.Segments.Select(WebMapper.RouteSegmentToRouteSegmentModel).ToList()
                });

                return Ok(outputResults);
            }
            catch (IncorrectPointPositionException ex)
            {
                var outputPoints = ex.PointsData.Select(x => new IncorrectPointPositionOutput
                {
                    UavId = x.Uav.Id,
                    Points = x.Points.Select(WebMapper.PointToPointModel)
                });

                return StatusCode((int)ValidationStatusCode.IncorectPositions, outputPoints);
            }
        }
    }
}