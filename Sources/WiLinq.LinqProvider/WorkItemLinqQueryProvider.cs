using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using WiLinq.LinqProvider.Extensions;

namespace WiLinq.LinqProvider
{
    internal class WorkItemLinqQueryProvider<T> : IWorkItemLinqQueryProvider where T:class
    {
        readonly WorkItemTrackingHttpClient _workItemTrackingHttpClient;
        readonly ProjectInfo _project;
        readonly ICustomWorkItemHelper<T> _creatorProvider;
        DateTime? _asOfDate;
        

        IQueryable<TOutput> IQueryProvider.CreateQuery<TOutput>(Expression expression)
        {
            return new WorkItemAsyncQuery<TOutput>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            throw new NotSupportedException();
        }

        TOutput IQueryProvider.Execute<TOutput>(Expression expression)
        {
            return (TOutput)Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {

            return Execute(expression);

        }

        public WorkItemLinqQueryProvider(WorkItemTrackingHttpClient workItemTrackingHttpClient, ProjectInfo project, ICustomWorkItemHelper<T> creatorProvider)
        {
            _workItemTrackingHttpClient = workItemTrackingHttpClient ?? throw new ArgumentNullException(nameof(workItemTrackingHttpClient));
            _project = project;
            _creatorProvider = creatorProvider;
        }

  

        public WorkItemLinqQueryProvider(WorkItemTrackingHttpClient workItemTrackingHttpClient)
            : this(workItemTrackingHttpClient, null, null)
        {

        }

        private IEnumerable<TResult> ApplySelect<TResult>(T[] workItems, Func<T, TResult> select)
        {
            return workItems.Select(select);
        }

        public string GetQueryText(Expression expression)
        {
            //QueryTranslator translator = new QueryTranslator();
            //QueryBuilder queryBuilder = translator.Translate(expression);

            //ConfigureExtraFilters(queryBuilder);

            //WorkItemQuery query = queryBuilder.BuildQuery(_server, _project, _asOfDate);
            //return query.WIQL;
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
           
            var translator = new QueryTranslator(_creatorProvider);
            var queryBuilder = translator.Translate(expression);

            ConfigureExtraFilters(queryBuilder);

            var query = queryBuilder.BuildQuery(_workItemTrackingHttpClient, _project, _asOfDate);

            var tmpResult = query.GetWorkItems();

            T[] wiResult;
            if (_creatorProvider != null)
            {
                wiResult = (from wi in tmpResult
                            select _creatorProvider.CreateItem(wi)).ToArray();
            }
            else
            {
                if (typeof(WorkItem) != typeof(T))
                {
                    throw new InvalidOperationException("creatorProvider required");
                }
                wiResult = tmpResult as T[];
            }


            if (queryBuilder.SelectLambda == null)
            {
                return wiResult;
            }

            var deleg = queryBuilder.SelectLambda.Compile();

            var applySelect = GetType().GetMethod("ApplySelect", BindingFlags.NonPublic | BindingFlags.Instance);

            var applySelectGeneric = applySelect.MakeGenericMethod(deleg.Method.ReturnType);

            var resultList = applySelectGeneric.Invoke(this, new object[] { wiResult, deleg });
            return resultList;
        }

        private void ConfigureExtraFilters(QueryBuilder queryBuilder)
        {
            if (_project != null)
            {
                queryBuilder.AddWhereClause(SystemField.Project + " = @project");
            }

            var attribs = typeof(T).GetCustomAttributes(typeof(WorkItemTypeAttribute), false) as WorkItemTypeAttribute[];

            if (attribs != null && attribs.Length == 1)
            {
                queryBuilder.AddWhereClause($"{SystemField.WorkItemType} = '{attribs[0].TypeName}'");
            }
        }

        public TPCQuery TransformAsWorkItemQuery(Expression expression)
        {
            return null;
        }

        public DateTime? AsOfDate
        {
            get => _asOfDate;
            set
            {
                if (_asOfDate.HasValue)
                {
                    throw new InvalidOperationException("AsOf Date already defined");
                }
                _asOfDate = value;
            }
        }
        
        public WorkItemTrackingHttpClient WorkItemTrackingHttpClient => _workItemTrackingHttpClient;

        public ProjectInfo Project => _project;
    }
}
