using Blazorise.DataGrid;
using Microsoft.AspNetCore.Components;
using RwaWebModels.ConcertContext;
using RwaWebModels.Interfaces;

namespace RwaWeb.Pages
{
    public partial class FetchData
    {
        [Inject]
        public IConcertContextService _concertApiAdapter { get; set; }

        private List<Concert> concertList;

        private int totalItems = 1000;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                concertList = (await _concertApiAdapter.GetUpcomingConcertsAsync(totalItems)).ToList();
            }
            catch (Exception)
            {
                concertList = new List<Concert>();
            }
            
            await base.OnInitializedAsync();
        }

        private async Task RetryOperation()
        {
            concertList = (await _concertApiAdapter.GetUpcomingConcertsAsync(totalItems)).ToList();
        }

        private async Task OnReadData(DataGridReadDataEventArgs<Concert> c)
        {
            if( c.CancellationToken.IsCancellationRequested == false)
            {
                var largeNumber = 10000; //just some large number for fetching all the items
                List<Concert> response;
                if (c.ReadDataMode is DataGridReadDataMode.Virtualize)
                {
                    response = (await _concertApiAdapter.GetUpcomingConcertsAsync(largeNumber)).Skip(c.VirtualizeOffset).Take(c.VirtualizeCount).ToList(); 
                }
                else if(c.ReadDataMode is DataGridReadDataMode.Paging)
                {
                    response = (await _concertApiAdapter.GetUpcomingConcertsAsync(largeNumber)).Skip((c.Page - 1) * c.PageSize).Take(c.PageSize).ToList();
                }
                else
                {
                    throw new Exception("Unhandled ReadDataMode");
                }

                if(c.CancellationToken.IsCancellationRequested == false)
                {
                    totalItems = response.Count; 
                    concertList = new List<Concert>(response);
                }
            }
        }

        private Task ClearConcerts()
        {
            concertList = new List<Concert>();
            return Task.CompletedTask;
        }
    }
}
