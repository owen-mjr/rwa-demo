using RwaWebModels.ConcertContext;

namespace RwaWebModels.Interfaces
{
    public interface IConcertContextService
    {
        Task<CreateResult> CreateConcertAsync(Concert newConcert);
        Task<UpdateResult> UpdateConcertAsync(Concert model);
        Task<DeleteResult> DeleteConcertAsync(int id);
        Task<Concert?> GetConcertByIdAsync(int id);
        Task<ICollection<Concert>> GetConcertsByIdAsync(ICollection<int> ids);
        Task<ICollection<Concert>> GetUpcomingConcertsAsync(int count);
    }
}
