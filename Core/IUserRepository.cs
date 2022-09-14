using Core.Entities;

namespace Core
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
        Task<List<User>> SearchByPartialUsernameAsync(string partialUsername, CancellationToken cancellationToken);
    }
}