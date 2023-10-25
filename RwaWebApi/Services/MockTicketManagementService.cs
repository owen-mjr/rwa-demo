using RwaWebApi.Services.Interfaces;
using RwaWebModels.TicketManagement;

namespace RwaWebApi.Services
{
    public class MockTicketManagementService : ITicketManagementService
    {
        public Task<CountAvailableTicketsResult> CountAvailableTicketsAsync(int concertId)
        {
            return Task.FromResult(new CountAvailableTicketsResult
            {
                CountOfAvailableTickets = 100,
            });
        }

        public Task<HaveTicketsBeenSoldResult> HaveTicketsBeenSoldAsync(int concertId)
        {
            return Task.FromResult(new HaveTicketsBeenSoldResult
            {
                HaveTicketsBeenSold = true,
            });
        }

        public Task<ReserveTicketsResult> ReserveTicketsAsync(int concertId, string userId, int numberOfTickets, int customerId)
        {
            return Task.FromResult(new ReserveTicketsResult
            {
                Status = ReserveTicketsResultStatus.Success,
            });
        }
    }
}
