using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task12.Models;

namespace Task12.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly TripContext _context;

        public ClientController(TripContext context)
        {
            _context = context;
        }

        // GET: /api/Client
        [HttpGet]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _context.Clients.ToListAsync();
            return Ok(clients);
        }

        // DELETE: /api/Client/{idClient}
        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var client = await _context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
                return NotFound("Client not found.");

            if (client.ClientTrips.Any())
                return BadRequest("Client has assigned trips and cannot be deleted.");

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }

       
        [HttpPost("{idClient}/trips/{idTrip}")]
        public async Task<IActionResult> AssignClientToTrip(int idClient, int idTrip)
        {
            var client = await _context.Clients.FindAsync(idClient);
            if (client == null)
                return NotFound("Client not found.");

            if (string.IsNullOrWhiteSpace(client.Pesel) || client.Pesel.Length != 11 || !client.Pesel.All(char.IsDigit))
                return BadRequest("Invalid PESEL. It must be 11 digits.");

            var trip = await _context.Trips
                .Include(t => t.ClientTrips)
                .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

            if (trip == null)
                return NotFound("Trip not found.");

            if (trip.DateFrom <= DateTime.Now)
                return BadRequest("Trip has already started.");

            bool alreadyAssigned = await _context.ClientTrips
                .AnyAsync(ct => ct.IdClient == idClient && ct.IdTrip == idTrip);

            if (alreadyAssigned)
                return BadRequest("Client is already assigned to this trip.");

            var clientTrip = new ClientTrip
            {
                IdClient = idClient,
                IdTrip = idTrip,
                RegisteredAt = DateTime.Now,
                PaymentDate = null 
            };

            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return Ok("Client successfully assigned to the trip.");
        }
    }
}
