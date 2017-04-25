using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace WiLinq.LinqProvider
{
    internal class Query<T> : IOrderedQueryable<T>       
    {
        readonly IWorkItemLinqQueryProvider _provider;
        readonly Expression _expression;
        DateTime? _asOfDate;

        public Query(IWorkItemLinqQueryProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            _expression = Expression.Constant(this);
        }

        public Query(IWorkItemLinqQueryProvider provider, Expression expression)
        {
            _expression = expression ?? throw new ArgumentNullException(nameof(expression));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            
          

        }



        Expression IQueryable.Expression => _expression;


        Type IQueryable.ElementType => typeof(T);


        IQueryProvider IQueryable.Provider => _provider;


        public IEnumerator<T> GetEnumerator()
        {

            return ((IEnumerable<T>)_provider.Execute(_expression)).GetEnumerator();

        }



        IEnumerator IEnumerable.GetEnumerator()
        {

            return ((IEnumerable)_provider.Execute(_expression)).GetEnumerator();

        }



        public override string ToString()
        {

            return _provider.GetQueryText(_expression);

        }

        public static TPCQuery TransformAsWorkItemQuery(IQueryable<T> query)
        {
            var wiQuery = query as Query<T>;
            if (wiQuery == null)
            {
                return null;
            }

            return wiQuery._provider.TransformAsWorkItemQuery(query.Expression);            
        }


        public Query<T> AsOf(DateTime dt)
        {

            if (_asOfDate.HasValue)
            {
                throw new InvalidOperationException("AsOf Date already defined");                

            }
            _provider.AsOfDate = dt;

            _asOfDate = dt;
            return this;

        }

    }
}
