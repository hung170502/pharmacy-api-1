using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pharmacy_API.Supports;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Repositories.Account
{
    public partial class UserRefreshTokenRepository : AbstractEfRepository<AccountContext, UserRefreshToken>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(AccountContext db, ILogger<UserRefreshTokenRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<UserRefreshToken> IncludeDeepObjects(IQueryable<UserRefreshToken> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<UserRefreshToken?> GetByIdAsync(string id, bool? isDeep = false)
        {
            IQueryable<UserRefreshToken> query = _db.UserRefreshTokens;
            query = query.Where(o => o.Id == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<UserRefreshToken>> GetListAsync(UserRefreshTokenFilter filter)
        {
            int total = 0;
            IQueryable<UserRefreshToken> query = _db.UserRefreshTokens;

            //query where

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.Id);
                total = await queryCount.CountAsync();
            }

            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy)
            {
                case "Id":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
                case "UserId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UserId) : query.OrderBy(o => o.UserId);
                    break;
                case "TokenId":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.TokenId) : query.OrderBy(o => o.TokenId);
                    break;
                case "RefreshToken":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.RefreshToken) : query.OrderBy(o => o.RefreshToken);
                    break;
                case "ExpiryTime":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.ExpiryTime) : query.OrderBy(o => o.ExpiryTime);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
            }
            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<UserRefreshToken>(total, await query.ToListAsync());
        }
        #endregion
    }
}