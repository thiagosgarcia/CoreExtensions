using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenguinSoft.CoreExtensions.Helpers.Request
{
    public class PagedResult<T>
    {
        public IQueryable<T> Items { get; set; }
        public IRequestParameters<T> Request { get; set; }

        public PagedResult(IEnumerable<T> list, IRequestParameters<T> request) :
            this(list?.AsQueryable(), request)
        {
        }
        public PagedResult(IQueryable<T> list, IRequestParameters<T> request)
        {
            Items = list;
            Request = request;
        }

        public static PagedResult<T> Create(IQueryable<T> items, IRequestParameters<T> request)
        {
            return new PagedResult<T>(items, request);
        }

        public static async Task<PagedResult<T>> CreateAsync(Task<IQueryable<T>> itens, IRequestParameters<T> request)
        {
            var i = await itens;
            return Create(i, request);
        }
    }
}
