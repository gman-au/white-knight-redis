using White.Knight.Domain;
using White.Knight.Tests.Domain;

namespace White.Knight.Redis.Tests.Integration
{
    public class RedisCustomerSpecByCustomerNameContains(string value)
        : SpecificationByTextContains<Customer>(o => o.CustomerName, value);
}