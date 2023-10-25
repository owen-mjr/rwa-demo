using RwaWebModels.TicketManagement;

namespace RwaWebApi.Services.Interfaces
{
    public interface ITicketManagementService
    {
        Task<CountAvailableTicketsResult> CountAvailableTicketsAsync(int concertId);
        Task<HaveTicketsBeenSoldResult> HaveTicketsBeenSoldAsync(int concertId);
        Task<ReserveTicketsResult> ReserveTicketsAsync(int concertId, string userId, int numberOfTickets, int customerId);
    }
}
