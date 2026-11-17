using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QLSK.Common.Exceptions;
using QLSK.Data;

namespace QLSK.Common.Inventory;

internal static class TransactionLock
{
    // SQL Server owns/releases this lock with the transaction, including rollback/disconnect.
    // Parameters are never concatenated into SQL. This also works across app processes.
    internal static async Task Acquire(ApplicationDbContext db, string resource, CancellationToken cancellationToken = default)
    {
        var transaction = db.Database.CurrentTransaction
            ?? throw new InvalidOperationException("A transaction is required for an inventory lock.");
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandTimeout = 7;
        command.CommandText = "DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000; SELECT @result;";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.DbType = DbType.String;
        parameter.Size = 255;
        parameter.Value = "QLSK:" + resource;
        command.Parameters.Add(parameter);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken.CanBeCanceled ? cancellationToken : db.RequestAborted));
        if (result < 0) throw new ApiException(503, "RESOURCE_BUSY", "Dữ liệu đang được xử lý. Vui lòng thử lại sau.");
    }
}
