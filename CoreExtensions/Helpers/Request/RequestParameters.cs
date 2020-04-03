using System;
using System.Collections.Generic;
using System.Linq;

namespace PenguinSoft.CoreExtensions.Helpers.Request
{
    public class RequestParameters<T> : IRequestParameters<T>
    {

        public int? PageId { get; set; }
        public int? PerPage { get; set; }
        public string SortField { get; set; }
        public bool? SortDirection { get; set; }
        public List<Filter<T>> Filters { get; set; }
        public int? ItemCount { get; set; }
        public int? PageCount { get; set; }
        public bool IncludeTotals { get; set; }



        public virtual IQueryable<T> GetQuery(IQueryable<T> query, bool includeTotals = false)
        {
            if (query == null)
                return query;

            query = GetFiltersQuery(query);
            query = GetSortQuery(query);
            query = GetPaginationQuery(query, includeTotals || IncludeTotals);

            return query;
        }

        protected virtual IQueryable<T> GetFiltersQuery(IQueryable<T> query)
        {
            if (Filters == null || Filters.All(x => x == null))
                return query;

            return Filters.Aggregate(query, (x, item) => item.GetQuery(x));
        }

        protected virtual IQueryable<T> GetSortQuery(IQueryable<T> query)
        {
            if (SortField == null)
                return query;

            var prop = typeof(T).GetProperty(SortField);
            if (SortDirection ?? true)
                query = query.OrderBy(x => prop.GetValue(x));
            else
                query = query.OrderByDescending(x => prop.GetValue(x));

            return query;
        }

        protected virtual IQueryable<T> GetPaginationQuery(IQueryable<T> query, bool includeTotals = false)
        {
            PageId = PageId == null || PageId < 0 ? 0 : PageId;
            PerPage = PerPage == null || PerPage <= 0 ? 10 : PerPage;

            ItemCount = null;
            PageCount = null;
            if (includeTotals)
            {
                ItemCount = query.Count();
                PageCount = (int)Math.Ceiling(((decimal)ItemCount) / PerPage.Value);
            }

            return query.Skip(PageId.Value * PerPage.Value).Take(PerPage.Value);
        }

    }
}