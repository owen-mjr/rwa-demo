using RwaWebApi.Data.Interfaces;

namespace RwaWebApi.Infrastructure
{
    public class ApplicationInitializer
    {
        private readonly IConcertRepository concertContextService;

        public ApplicationInitializer(IConcertRepository concertContextService)
        {
            this.concertContextService = concertContextService;
        }

        public void Initialize()
        {
            // Initialize all resources at application startup.
            concertContextService.Initialize();
        }
    }
}
