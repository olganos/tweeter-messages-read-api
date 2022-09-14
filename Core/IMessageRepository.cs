using Core.Entities;

namespace Core
{
    public interface IMessageRepository
    {
        Task<List<Tweet>> GetAllAsync(CancellationToken cancellationToken);
        Task<List<Tweet>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    }
}