using Microsoft.AspNetCore.Mvc;
using EINAR5Integration.models;
using EINAR5Integration.Services;
using System.Diagnostics;

namespace EINAR5Integration.Controllers
{
    public class DeviceController : Controller
    {

        private readonly DeviceService _deviceService;

        public DeviceController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost]
        [Route("api/[controller]/Login")]
        [Produces("application/json")]
        public IActionResult Login([FromBody] AuthRequest request)
        {
            var (isSuccessful, message) = _deviceService.Login(request);
            if (isSuccessful)
            {
                return Ok(new { message = message });
            }
            else
            {
                return BadRequest(new { message = message });
            }
        }

        [HttpPost]
        [Route("api/[controller]/GetEvents")]
        [Produces("application/json")]
        public IActionResult GetEvents([FromBody] EventRequest request)
        {

            Stopwatch stopwatch = Stopwatch.StartNew();

            stopwatch.Start();
            var (isSuccessful, events, errorMessage) = _deviceService.GetEvents(request);
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds / 1000.0; // in seconds
            Console.WriteLine($"Time taken for GetEvents request: {elapsed} s");

            if (isSuccessful)
            {
                if (events.Count == 0)
                {
                    return Ok(new { message = "No events were found in the specified time range." });
                }
                else
                {
                    return Ok(events);
                }
            }
            else
            {
                return BadRequest(new { message = errorMessage });
            }
        }
            
    }
} 
