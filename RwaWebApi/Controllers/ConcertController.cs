using Microsoft.AspNetCore.Mvc;
using RwaWebApi.Data.Interfaces;
using RwaWebApi.Extenstions;
using RwaWebApi.Services.Interfaces;
using RwaWebModels.ConcertContext;
using RwaWebModels.Interfaces;
using System.Net;
using System.Net.Mime;
using System.Text.Json;

namespace RwaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcertController : ControllerBase
    {
        public const int DefaultNumberOfConcerts = 10;

        private readonly ILogger<ConcertController> _logger;
        private readonly ITicketManagementService _ticketService;
        private readonly IConcertRepository _concertRepository;

        public ConcertController(ILogger<ConcertController> logger, ITicketManagementService ticketService, IConcertRepository concertRepository)
        {
            _logger = logger;
            _ticketService = ticketService;
            _concertRepository = concertRepository;
        }

        [HttpGet("{id}", Name = "GetConcertById")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Concert))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAsync(int id)
        {
            try
            {
                var concert = await _concertRepository.GetConcertByIdAsync(id);
                if (concert == null)
                {
                    return NotFound();
                }
                concert.NumberOfTicketsForSale = await CountAvailableTicketsAsync(concert.Id);

                return Ok(concert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception from {nameof(GetAsync)}");
                return Problem("Unable to Get the concert");
            }
        }

        [HttpPost(Name = "CreateConcert")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Concert))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(CreateResult))]
        public async Task<IActionResult> CreateAsync(Concert model)
        {
            try
            {
                if (ModelState.IsValid == false)
                {
                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ConvertToErrorMessages()
                    });
                }
                else if (model.IsVisible && await AreTickeketsAvailableAsync(model.Id) == false)
                {
                    return BadRequest(new CreateResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ServerError("Cannot make a concert visible if tickets are not available for sale")
                    });
                }

                var newConcertResult = await _concertRepository.CreateConcertAsync(model);
                return CreatedAtRoute("GetConcertById", new { id = newConcertResult.NewId }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception from {nameof(CreateAsync)}");
                return Problem("Unable to Create the concert");
            }
        }

        [HttpPut(Name = "UpdateConcert")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(Concert))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(UpdateResult))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAsync(Concert model)
        {
            try
            {
                var existingConcert = await _concertRepository.GetConcertByIdAsync(model.Id);
                if (existingConcert == null)
                {
                    return NotFound();
                }
                else if (ModelState.IsValid == false)
                {
                    return BadRequest(new UpdateResult 
                    { 
                        Success = false, 
                        ErrorMessages = ModelState.ConvertToErrorMessages() 
                    });
                }
                else if (model.IsVisible && model.NumberOfTicketsForSale != await CountAvailableTicketsAsync(model.Id))
                {
                    return BadRequest(new UpdateResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ServerError("Cannot change count of available tickets while concert is visible")
                    });
                }
                else if (model.IsVisible && await AreTickeketsAvailableAsync(model.Id) == false) 
                {
                    return BadRequest(new UpdateResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ServerError("Cannot make a concert visible if tickets are not available for sale")
                    });
                }

                await _concertRepository.UpdateConcertAsync(model);
                return Accepted(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception from {nameof(UpdateAsync)}");
                return Problem("Unable to Update the concert");
            }
        }

        [HttpDelete("{id}", Name = "DeleteConcert")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(DeleteResult))]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var concert = await _concertRepository.GetConcertByIdAsync(id);

                if (concert == null)
                {
                    return NotFound();
                }
                else if (concert.IsVisible)
                {
                    return BadRequest(new DeleteResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ServerError("Visible concerts cannot be deleted")
                    });
                }
                else if (await HaveTicketsBeenSoldAsync(concert.Id))
                {
                    return BadRequest(new DeleteResult
                    {
                        Success = false,
                        ErrorMessages = ModelState.ServerError("Cannot delete a concert that has sold tickets")
                    });
                }

                await _concertRepository.DeleteConcertAsync(concert.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled exception from {nameof(DeleteAsync)}");
                return Problem("Unable to Delete the concert");
            }
        }

        [HttpGet("GetUpcomingConcerts/{numberOfConcerts?}", Name = "GetUpcomingConcerts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<Concert>))]
        public async IAsyncEnumerable<Concert> GetUpcomingConcertsAsync(int numberOfConcerts = DefaultNumberOfConcerts)
        {
            var concerts = await _concertRepository.GetUpcomingConcertsAsync(numberOfConcerts);

            foreach (var concert in concerts)
            {
                yield return concert;
            }
        }

        [HttpGet("GetConcertsByIds", Name = "GetConcertsByIds")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICollection<Concert>))]
        public async IAsyncEnumerable<Concert> GetConcertsByIdAsync(string listOfIds)
        {
            List<int>? ids;
            try
            {
                ids = JsonSerializer.Deserialize<List<int>>(listOfIds);
            }
            catch
            {
                ids = new List<int>();
            }

            var concerts = await _concertRepository.GetConcertsByIdAsync(ids!);

            foreach (var concert in concerts)
            {
                yield return concert;
            }
        }

        private async Task<bool> HaveTicketsBeenSoldAsync(int concertId)
        {
            var result = await _ticketService.HaveTicketsBeenSoldAsync(concertId);
            TicketManagementResultGuardClause(result);
            return result.HaveTicketsBeenSold;
        }

        private async Task<bool> AreTickeketsAvailableAsync(int concertId)
        {
            return await CountAvailableTicketsAsync(concertId) > 0;
        }

        private async Task<int> CountAvailableTicketsAsync(int concertId)
        {
            var result = await _ticketService.CountAvailableTicketsAsync(concertId);
            TicketManagementResultGuardClause(result);
            return result.CountOfAvailableTickets;
        }

        private static void TicketManagementResultGuardClause<T>(T result) where T : IServiceProviderResult
        {
            if (string.IsNullOrEmpty(result.ErrorMessage) == false)
            {
                throw new InvalidOperationException("Error invoking ticket management service", new WebException(result.ErrorMessage));
            }
        }
    }
}