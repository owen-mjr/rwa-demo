using RwaWebModels.Interfaces;

namespace RwaWebApi.Data.Interfaces
{
    public interface IConcertRepository : IConcertContextService
    {
        public void Initialize();
    }
}
