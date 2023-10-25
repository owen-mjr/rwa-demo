using RwaWebModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.
    Threading.Tasks;

namespace RwaWebModels.TicketManagement
{
    public class ReserveTicketsResult : IServiceProviderResult
    {
        public ICollection<string> TicketNumbers { get; set; } = new List<string>();

        public ReserveTicketsResultStatus Status { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
