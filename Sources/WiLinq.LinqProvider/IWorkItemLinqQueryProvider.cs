using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace WiLinq.LinqProvider
{
    internal interface IWorkItemLinqQueryProvider : IQueryProvider
    {
        DateTime? AsOfDate { get; set; }
        string GetQueryText(Expression expression);
        TPCQuery TransformAsWorkItemQuery(Expression expression);

        Task<object> ExecuteAync(Expression expression);
        Task<List<int>> ExecuteAndGetIdsAsync(Expression expression);
    }
}