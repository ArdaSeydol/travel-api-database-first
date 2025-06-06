using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task12.Models;
using Task12.DTOs;

namespace Task12.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly TripContext _context;

    public TripController(TripContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10)
    {
        int totalTrips = await _context.Trips.CountAsync();
        int totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

        var trips = await _context.Trips
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.IdClientNavigation)
            .Include(t => t.IdCountries)
            .Skip((pageNum - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var tripDtos = trips.Select(t => new TripDto
        {
            Name = t.Name,
            Description = t.Description,
            DateFrom = t.DateFrom.ToString("yyyy-MM-dd"),
            DateTo = t.DateTo.ToString("yyyy-MM-dd"),
            MaxPeople = t.MaxPeople,
            Countries = t.IdCountries.Select(c => new CountryDto
            {
                Name = c.Name
            }).ToList(),
            Clients = t.ClientTrips.Select(ct => new ClientDto
            {
                FirstName = ct.IdClientNavigation.FirstName,
                LastName = ct.IdClientNavigation.LastName
            }).ToList()
        }).ToList();

        var response = new TripResponseDto
        {
            PageNum = pageNum,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = tripDtos
        };

        return Ok(response);
    }
    
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AddClientToTripDto dto)
    {
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        if (trip == null)
            return NotFound("Trip not found.");

        if (trip.DateFrom <= DateTime.Now)
            return BadRequest("Cannot register for a trip that has already started.");

        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
        if (existingClient != null)
        {
            bool alreadyRegistered = await _context.ClientTrips
                .AnyAsync(ct => ct.IdClient == existingClient.IdClient && ct.IdTrip == idTrip);
            if (alreadyRegistered)
                return BadRequest("Client already registered for this trip.");
        }

        if (existingClient == null)
        {
            existingClient = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();
        }

        var newClientTrip = new ClientTrip
        {
            IdClient = existingClient.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = dto.PaymentDate
        };

        _context.ClientTrips.Add(newClientTrip);
        await _context.SaveChangesAsync();

        return Ok("Client successfully assigned to trip.");
    }
}
