using System.Collections.Generic;
using System.Linq;

namespace PenguinSoft.CoreExtensions.Helpers.Request
{
    public interface IRequestParameters<T>
    {
        int? PageId { get; set; }
        int? PerPage { get; set; }
        string SortField { get; set; }
        bool? SortDirection { get; set; }
        int? ItemCount { get; set; }
        int? PageCount { get; set; }
        List<Filter<T>> Filters { get; set; }

        IQueryable<T> GetQuery(IQueryable<T> query, bool includeTotals = false);
    }
}