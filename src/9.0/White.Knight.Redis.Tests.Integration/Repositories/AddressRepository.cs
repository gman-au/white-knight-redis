using System;
using System.Linq.Expressions;
using White.Knight.Redis.Attribute;
using White.Knight.Redis.Options;
using White.Knight.Tests.Domain;

namespace White.Knight.Redis.Tests.Integration.Repositories
{
    [IsRedisRepository]
    public class AddressRepository(RedisRepositoryFeatures<Address> repositoryFeatures)
        : RedisKeylessRepositoryBase<Address>(repositoryFeatures)
    {
        public override Expression<Func<Address, object>> DefaultOrderBy() => o => o.AddressId;
    }
}