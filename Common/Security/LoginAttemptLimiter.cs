using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using QLSK.Common.Exceptions;

namespace QLSK.Common.Security;

public sealed class LoginAttemptLimiter : IDisposable
{
    // Partition keys contain a digest, not an email address. Idle partitions are retired by the framework.
    private readonly PartitionedRateLimiter<string> limiter = PartitionedRateLimiter.Create<string, string>(email =>
        RateLimitPartition.GetFixedWindowLimiter(Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email))), _ => new()
        {
            PermitLimit = 10, Window = TimeSpan.FromMinutes(5), QueueLimit = 0, AutoReplenishment = true
        }));

    public void Check(string email)
    {
        using var lease = limiter.AttemptAcquire(email);
        if (!lease.IsAcquired)
            throw new ApiException(429, "LOGIN_RATE_LIMITED", "Đã thử đăng nhập quá nhiều lần. Vui lòng thử lại sau 5 phút.");
    }

    public void Dispose() => limiter.Dispose();
}
