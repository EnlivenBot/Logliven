using Logliven.Common;
using Microsoft.EntityFrameworkCore;

namespace Logliven.Postgres.Extensions;

public static class QueryableExtensions {
    public static async Task EnsureExistsAsync<T>(this IQueryable<T> queryable, CancellationToken token) {
        if (!await queryable.AnyAsync(cancellationToken: token)) {
            throw new NotFoundException();
        }
    }
}