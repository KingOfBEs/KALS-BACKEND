using KALS.Domain.DataAccess;
using KALS.Domain.Entities;
using KALS.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace KALS.Repository.Implement;

public class UserRepository: GenericRepository<User>, IUserRepository
{
    public UserRepository(DbContext context) : base(context)
    {
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await SingleOrDefaultAsync(
            predicate: x => x.Id == id
        );
        return user;
    }

    public async Task<User> GetUserByUsernameOrPhoneNumber(string usernameOrPhoneNumber)
    {
        var user = await SingleOrDefaultAsync(
            predicate: x => x.Username == usernameOrPhoneNumber || x.PhoneNumber == usernameOrPhoneNumber
        );
        return user;
    }

    public async Task<ICollection<User>> GetAllUsers()
    {
        var users = await GetListAsync();
        return users;
    }

    public async Task<User> GetUserByPhoneNumber(string phoneNumber)
    {
        return await SingleOrDefaultAsync(
            predicate: x => x.PhoneNumber == phoneNumber
        );
    }
    
}