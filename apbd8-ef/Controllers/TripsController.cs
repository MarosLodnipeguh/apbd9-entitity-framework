using apbd8_ef.Data;
using apbd8_ef.DTOs;
using apbd8_ef.Models;
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

    [Route("/trips")]
    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
    {
        
        var sortedTrips = _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Select(t => new
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(cl => new
                {
                    FirstName = cl.IdClientNavigation.FirstName,
                    LastName = cl.IdClientNavigation.LastName,
                }).ToList()
            });

        var totalTrips = await sortedTrips.CountAsync();
        
        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);
        
        if (page > totalPages)
        {
            return BadRequest("Page number is too high");
        }
        
        var trips = await sortedTrips
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var response = new
        {
            pageNum = page,
            pageSize = pageSize,
            TotalPages = totalPages,
            Trips = trips
        };
        
        return Ok(response);
    }
    
    [HttpDelete("/clients/{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        
        // check if client exists
        if (client == null)
        {
            return NotFound();
        }
        
        // check if client has any trips
        bool hasTrips = await _context.ClientTrips.AnyAsync(t => t.IdClient == id);
        if (hasTrips)
        {
            return BadRequest("Client has trips assigned");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return Ok(client);
    }
    
    [HttpPost("/trips/{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTrip([FromBody] TripRequestDto tripRequest)
    {
        // check if sent json is valid
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        // check if client exists
        bool clientExists = await _context.Clients.AnyAsync(c => c.Pesel == tripRequest.Pesel);
        if (clientExists)
        {
            return BadRequest("Client already exists");
        }
        
        // check if client is assigned to a trip
        bool clientAssigned = await _context.ClientTrips.AnyAsync(ct => ct.IdClientNavigation.Pesel == tripRequest.Pesel);
        if (clientAssigned)
        {
            return BadRequest("Client is already assigned to a trip");
        }
        
        // check if trip exists
        var trip = await _context.Trips.FindAsync(tripRequest.IdTrip);
        if (trip == null)
        {
            return NotFound("Trip not found");
        }
        
        // check if trip is in the future
        if (trip.DateFrom < DateTime.Now)
        {
            return BadRequest("Trip has already started");
        }
        
        var client = new Client
        {
            FirstName = tripRequest.FirstName,
            LastName = tripRequest.LastName,
            Email = tripRequest.Email,
            Telephone = tripRequest.Telephone,
            Pesel = tripRequest.Pesel
        };
        
        var clientTrip = new ClientTrip
        {
            IdClientNavigation = client,
            IdTripNavigation = trip,
            PaymentDate = tripRequest.PaymentDate, // nullable
            RegisteredAt = DateTime.Now
        };
        
        await _context.Clients.AddAsync(client);
        await _context.ClientTrips.AddAsync(clientTrip);
        await _context.SaveChangesAsync();
        
        return Ok("Client created and added to the trip");
    }
    
}