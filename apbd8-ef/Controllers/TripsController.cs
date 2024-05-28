using apbd8_ef.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly ApbdContext _context;
    public TripsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _context.Trips
            .Select( e => new
            {
                Name = e.Name,
                Countries = e.IdCountries.Select( c => new
                {
                    Name = c.Name
                })
            }).ToListAsync();
        
        return Ok(trips);
    }
}