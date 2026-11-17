using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace QLSK.Tests;

public sealed class DatabaseQueryCounter : DbCommandInterceptor
{
    private int count;
    public int Count => Volatile.Read(ref count);
    public string LastCommand { get; private set; } = "";
    public void Reset() => Interlocked.Exchange(ref count, 0);
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref count);
        LastCommand = command.CommandText;
        return ValueTask.FromResult(result);
    }
}
