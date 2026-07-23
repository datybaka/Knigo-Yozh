using Microsoft.EntityFrameworkCore;
using KnigoYozh.Domain.Entities;

namespace KnigoYozh.Application.Interfaces;

public interface IKnigoYozhDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
