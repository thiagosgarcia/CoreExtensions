using System.Linq;
using System.Threading.Tasks;
using PenguinSoft.CoreExtensions.Helpers;
using PenguinSoft.CoreExtensions.Helpers.Request;

namespace PenguinSoft.CoreExtensions.Extensions

{
    public static class PagedResultExtensions
    {
        public static PagedResult<T> ToPagedResult<T>(this IQueryable<T> list, IRequestParameters<T> request) where T : Entity
        {
            return PagedResult<T>.Create(list, request);
        }

        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this Task<IQueryable<T>> list, IRequestParameters<T> request) where T : Entity
        {
            return await PagedResult<T>.CreateAsync(list, request);
        }
    }
}
