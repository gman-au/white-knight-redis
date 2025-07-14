using System;
using System.Linq.Expressions;
using White.Knight.Redis.Attribute;
using White.Knight.Redis.Options;
using White.Knight.Tests.Domain;

namespace White.Knight.Redis.Tests.Integration.Repositories
{
    [IsRedisRepository]
    public class CustomerRepository(RedisRepositoryFeatures<Customer> repositoryFeatures)
        : RedisRepositoryBase<Customer>(repositoryFeatures)
    {
        public override Expression<Func<Customer, object>> KeyExpression() => b => b.CustomerId;
    }
}