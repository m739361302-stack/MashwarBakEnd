using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Application.Services.helper
{
    public static class QueryableExt
    {
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> q,
            bool condition,
            Expression<Func<T, bool>> predicate)
            => condition ? q.Where(predicate) : q;
    }
}
