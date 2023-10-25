using RwaWebModels.Interfaces;

namespace RwaWebModels.TicketManagement
{
    public class HaveTicketsBeenSoldResult : IServiceProviderResult
    {
        public bool HaveTicketsBeenSold { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
