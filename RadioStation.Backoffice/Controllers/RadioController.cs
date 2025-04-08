using Microsoft.AspNetCore.Mvc;
using RadioStationServer.GrpcProto;

namespace RadioStation.Backoffice.Controllers;

[Route("radio")]
public class RadioController(
    RadioControl.RadioControlClient grpcClient,
    ILogger<RadioController> logger)
    : Controller
{
    [HttpPost("start")]
    public async Task<IActionResult> Start()
    {
        try
        {
            var response = await grpcClient.StartStreamAsync(new StreamRequest());
            return Json(new { 
                success = response.Success, 
                message = response.Message 
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting stream");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("stop")]
    public async Task<IActionResult> Stop()
    {
        var response = await grpcClient.StopStreamAsync(new StopRequest());
        return Json(new { 
            success = response.Success, 
            message = response.Message 
        });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var status = await grpcClient.GetStreamStatusAsync(new StatusRequest());
        return Json(new {
            isPlaying = status.IsPlaying,
            currentTrack = status.CurrentTrack,
            listeners = status.ListenersCount
        });
    }
}