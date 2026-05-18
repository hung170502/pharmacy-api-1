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
    public partial class UserRepository : AbstractEfRepository<AccountContext, ApplicationUser>, IUserRepository
    {
        public UserRepository(AccountContext db, ILogger<UserRepository> logger) : base(db, logger)
        {

        }

        private IQueryable<ApplicationUser> IncludeDeepObjects(IQueryable<ApplicationUser> query)
        {
            //return query.Include(o => o.ReferTable);
            return query;
        }

        #region Get By Id
        public async Task<ApplicationUser?> GetByIdAsync(string id, bool? isDeep = false)
        {
            IQueryable<ApplicationUser> query = _db.Users;
            query = query.Where(o => o.Id == id);

            if (isDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            return await query.SingleOrDefaultAsync();
        }
        #endregion

        #region Get List
        public async Task<PagedDto<ApplicationUser>> GetListAsync(UserFilter filter)
        {
            int total = 0;
            IQueryable<ApplicationUser> query = _db.Users;

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                string keyword = filter.Keyword.ToLower();
                query = query.Where(user =>
                    user.UserName.ToLower().Contains(keyword) ||
                    user.Email.ToLower().Contains(keyword) ||
                    user.PhoneNumber.ToLower().Contains(keyword));
            }

            if (filter.IsOutputTotal)
            {
                var queryCount = query.Select(o => o.Id);
                total = await queryCount.CountAsync();
            }

            if (filter.IsDeep.Equals(true))
            {
                query = IncludeDeepObjects(query);
            }

            switch (filter.OrderBy?.ToLower())
            {
                case "id":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
                case "username":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.UserName) : query.OrderBy(o => o.UserName);
                    break;
                case "normalizeduserName":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.NormalizedUserName) : query.OrderBy(o => o.NormalizedUserName);
                    break;
                case "email":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Email) : query.OrderBy(o => o.Email);
                    break;
                case "normalizedemail":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.NormalizedEmail) : query.OrderBy(o => o.NormalizedEmail);
                    break;
                case "emailconfirmed":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.EmailConfirmed) : query.OrderBy(o => o.EmailConfirmed);
                    break;
                case "passwordhash":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PasswordHash) : query.OrderBy(o => o.PasswordHash);
                    break;
                case "securitystamp":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.SecurityStamp) : query.OrderBy(o => o.SecurityStamp);
                    break;
                case "concurrencystamp":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.ConcurrencyStamp) : query.OrderBy(o => o.ConcurrencyStamp);
                    break;
                case "phonenumber":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PhoneNumber) : query.OrderBy(o => o.PhoneNumber);
                    break;
                case "phonenumberconfirmed":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.PhoneNumberConfirmed) : query.OrderBy(o => o.PhoneNumberConfirmed);
                    break;
                case "twofactorenabled":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.TwoFactorEnabled) : query.OrderBy(o => o.TwoFactorEnabled);
                    break;
                case "lockoutend":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.LockoutEnd) : query.OrderBy(o => o.LockoutEnd);
                    break;
                case "lockoutenabled":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.LockoutEnabled) : query.OrderBy(o => o.LockoutEnabled);
                    break;
                case "accessfailedcount":
                    query = filter.IsDescending ? query.OrderByDescending(o => o.AccessFailedCount) : query.OrderBy(o => o.AccessFailedCount);
                    break;

                default:
                    query = filter.IsDescending ? query.OrderByDescending(o => o.Id) : query.OrderBy(o => o.Id);
                    break;
            }

            query = query.Skip(filter.GetSkip()).Take(filter.GetTake());

            return new PagedDto<ApplicationUser>(total, await query.ToListAsync());
        }

        #endregion

        #region Random code
        public string GenerateRandomCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }
        #endregion
    }
}