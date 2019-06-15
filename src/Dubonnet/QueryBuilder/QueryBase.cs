using System;
using System.Collections.Generic;
using System.Linq;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public abstract class AbstractQuery
    {
        protected AbstractQuery Parent;
        public Action<string> Log { get; set; }
    }

    public abstract partial class QueryBase<Q> : AbstractQuery where Q : QueryBase<Q>
    {
        protected Q instance;
        public List<AbstractClause> Clauses { get; set; } = new List<AbstractClause>();
        
        private bool orFlag = false;
        private bool notFlag = false;
        public string EngineScope = null;

        public QueryBase()
        {
        }

        public Q SetEngineScope(string engine)
        {
            instance.EngineScope = engine;
            return instance;
        }

        public void SetLogAction(Action<string> log)
        {
            instance.Log = log;
        }

        public abstract Q NewQuery();
        
        public abstract string GetAlias();

        /// <summary>
        /// Return a cloned copy of the current factory.
        /// </summary>
        /// <returns></returns>
        public virtual Q Clone()
        {
            var q = NewQuery();

            q.Clauses = instance.Clauses.Select(x => x.Clone()).ToList();

            return q;
        }

        public Q SetParent(AbstractQuery parent)
        {
            if (instance == parent)
            {
                throw new ArgumentException("Cannot set the same factory as a parent of itself");
            }

            instance.Parent = parent;
            return instance;
        }

        public Q NewChild()
        {
            var newQuery = NewQuery().SetParent(instance);
            newQuery.EngineScope = instance.EngineScope;
            return newQuery;
        }

        /// <summary>
        /// Add a component clause to the factory.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="clause"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Q AddComponent(string component, AbstractClause clause, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            clause.Engine = engineCode;
            clause.Component = component;
            Clauses.Add(clause);

            return instance;
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <returns></returns>
        public List<C> GetComponents<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            var clauses = Clauses
                .Where(x => x.Component == component)
                .Where(x => engineCode == null || x.Engine == null || engineCode == x.Engine)
                .Cast<C>();

            return clauses.ToList();
        }

        /// <summary>
        /// Get the list of clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public List<AbstractClause> GetComponents(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Get a single component clause from the factory.
        /// </summary>
        /// <returns></returns>
        public C GetOneComponent<C>(string component, string engineCode = null) where C : AbstractClause
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents<C>(component, engineCode)
            .FirstOrDefault();
        }

        /// <summary>
        /// Get a single component clause from the factory.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public AbstractClause GetOneComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetOneComponent<AbstractClause>(component, engineCode);
        }

        /// <summary>
        /// Return wether the factory has clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public bool HasComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            return GetComponents(component, engineCode).Any();
        }

        /// <summary>
        /// Remove all clauses for a component.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="engineCode"></param>
        /// <returns></returns>
        public Q ClearComponent(string component, string engineCode = null)
        {
            if (engineCode == null)
            {
                engineCode = EngineScope;
            }

            Clauses = Clauses
                .Where(x => !(x.Component == component && (engineCode == null || x.Engine == null || engineCode == x.Engine)))
                .ToList();

            return instance;
        }

        /// <summary>
        /// Set the next boolean operator to "and" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Q And()
        {
            orFlag = false;
            return instance;
        }

        /// <summary>
        /// Set the next boolean operator to "or" for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Q Or()
        {
            orFlag = true;
            return instance;
        }

        /// <summary>
        /// Set the next "not" operator for the "where" clause.
        /// </summary>
        /// <returns></returns>
        public Q Not(bool flag = true)
        {
            notFlag = flag;
            return instance;
        }

        /// <summary>
        /// Get the boolean operator and reset it to "and"
        /// </summary>
        /// <returns></returns>
        public bool GetOr()
        {
            var ret = orFlag;

            // reset the flag
            orFlag = false;
            return ret;
        }

        /// <summary>
        /// Get the "not" operator and clear it
        /// </summary>
        /// <returns></returns>
        public bool GetNot()
        {
            var ret = notFlag;

            // reset the flag
            notFlag = false;
            return ret;
        }

        public Q FromRaw(string expression, params object[] bindings)
        {
            return ClearComponent("from").AddComponent("from", new RawFromClause
            {
                Expression = expression,
                Bindings = Helper.Flatten(bindings).ToArray()
            });
        }

        /// <summary>
        /// Add a from Clause
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Q From(string table)
        {
            return ClearComponent("from").AddComponent("from", new FromClause
            {
                Table = table
            });
        }

        public Q From(Q factory)
        {
            factory.SetParent(instance);
            return ClearComponent("from").AddComponent("from", new QueryFromClause<Q>
            {
                Query = factory
            });
        }

        public Q From(Func<Q, Q> callback)
        {
            var query = NewQuery();
            query.SetParent(instance);
            return From(callback.Invoke(query));
        }
    }
}