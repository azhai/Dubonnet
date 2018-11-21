using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        public Q Having(string column, string op, object value)
        {

            // If the value is "null", we will just assume the developer wants to add a
            // Having null clause to the factory. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return instance.Not(op != "=").HavingNull(column);
            }

            return instance.AddComponent("having", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNot(string column, string op, object value)
        {
            return instance.Not().Having(column, op, value);
        }

        public Q OrHaving(string column, string op, object value)
        {
            return instance.Or().Having(column, op, value);
        }

        public Q OrHavingNot(string column, string op, object value)
        {
            return instance.Or().Not().Having(column, op, value);
        }

        public Q Having(string column, object value)
        {
            return instance.Having(column, "=", value);
        }
        public Q HavingNot(string column, object value)
        {
            return instance.HavingNot(column, "=", value);
        }
        public Q OrHaving(string column, object value)
        {
            return instance.OrHaving(column, "=", value);
        }
        public Q OrHavingNot(string column, object value)
        {
            return instance.OrHavingNot(column, "=", value);
        }

        /// <summary>
        /// Perform a Having constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Q Having(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return instance.Having(dictionary);
        }

        public Q Having(IReadOnlyDictionary<string, object> values)
        {
            var query = instance;
            var orFlag = instance.GetOr();
            var notFlag = instance.GetNot();

            foreach (var tuple in values)
            {
                if (orFlag)
                {
                    query = query.Or();
                }
                else
                {
                    query.And();
                }

                query = instance.Not(notFlag).Having(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Q HavingRaw(string sql, params object[] bindings)
        {
            return instance.AddComponent("having", new RawCondition
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q OrHavingRaw(string sql, params object[] bindings)
        {
            return instance.Or().HavingRaw(sql, bindings);
        }

        /// <summary>
        /// Apply a nested Having clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Having(Func<Q, Q> callback)
        {
            var query = callback.Invoke(instance.NewChild());

            return instance.AddComponent("having", new NestedCondition<Q>
            {
                Query = query,
                IsNot = instance.GetNot(),
                IsOr = instance.GetOr(),
            });
        }

        public Q HavingNot(Func<Q, Q> callback)
        {
            return instance.Not().Having(callback);
        }

        public Q OrHaving(Func<Q, Q> callback)
        {
            return instance.Or().Having(callback);
        }

        public Q OrHavingNot(Func<Q, Q> callback)
        {
            return instance.Not().Or().Having(callback);
        }

        public Q HavingColumns(string first, string op, string second)
        {
            return instance.AddComponent("having", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q OrHavingColumns(string first, string op, string second)
        {
            return instance.Or().HavingColumns(first, op, second);
        }

        public Q HavingNull(string column)
        {
            return instance.AddComponent("having", new NullCondition
            {
                Column = column,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNotNull(string column)
        {
            return instance.Not().HavingNull(column);
        }

        public Q OrHavingNull(string column)
        {
            return instance.Or().HavingNull(column);
        }

        public Q OrHavingNotNull(string column)
        {
            return instance.Or().Not().HavingNull(column);
        }

        public Q HavingTrue(string column)
        {
            return instance.AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = true,
            });
        }

        public Q OrHavingTrue(string column)
        {
            return instance.Or().HavingTrue(column);
        }

        public Q HavingFalse(string column)
        {
            return instance.AddComponent("having", new BooleanCondition
            {
                Column = column,
                Value = false,
            });
        }

        public Q OrHavingFalse(string column)
        {
            return instance.Or().HavingFalse(column);
        }

        public Q HavingLike(string column, string value, bool caseSensitive = false)
        {
            return instance.AddComponent("having", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return instance.Not().HavingLike(column, value, caseSensitive);
        }

        public Q OrHavingLike(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().HavingLike(column, value, caseSensitive);
        }

        public Q OrHavingNotLike(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().Not().HavingLike(column, value, caseSensitive);
        }
        public Q HavingStarts(string column, string value, bool caseSensitive = false)
        {
            return instance.AddComponent("having", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return instance.Not().HavingStarts(column, value, caseSensitive);
        }

        public Q OrHavingStarts(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().HavingStarts(column, value, caseSensitive);
        }

        public Q OrHavingNotStarts(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().Not().HavingStarts(column, value, caseSensitive);
        }

        public Q HavingEnds(string column, string value, bool caseSensitive = false)
        {
            return instance.AddComponent("having", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return instance.Not().HavingEnds(column, value, caseSensitive);
        }

        public Q OrHavingEnds(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().HavingEnds(column, value, caseSensitive);
        }

        public Q OrHavingNotEnds(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().Not().HavingEnds(column, value, caseSensitive);
        }

        public Q HavingContains(string column, string value, bool caseSensitive = false)
        {
            return instance.AddComponent("having", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }

        public Q HavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return instance.Not().HavingContains(column, value, caseSensitive);
        }

        public Q OrHavingContains(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().HavingContains(column, value, caseSensitive);
        }

        public Q OrHavingNotContains(string column, string value, bool caseSensitive = false)
        {
            return instance.Or().Not().HavingContains(column, value, caseSensitive);
        }

        public Q HavingBetween<T>(string column, T lower, T higher)
        {
            return instance.AddComponent("having", new BetweenCondition<T>
            {
                Column = column,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public Q OrHavingBetween<T>(string column, T lower, T higher)
        {
            return instance.Or().HavingBetween(column, lower, higher);
        }
        public Q HavingNotBetween<T>(string column, T lower, T higher)
        {
            return instance.Not().HavingBetween(column, lower, higher);
        }
        public Q OrHavingNotBetween<T>(string column, T lower, T higher)
        {
            return instance.Or().Not().HavingBetween(column, lower, higher);
        }

        public Q HavingIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string most probably he wants List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                string val = values as string;

                return instance.AddComponent("having", new InCondition<string>
                {
                    Column = column,
                    IsOr = instance.GetOr(),
                    IsNot = instance.GetNot(),
                    Values = new List<string> { val }
                });
            }

            return instance.AddComponent("having", new InCondition<T>
            {
                Column = column,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
                Values = values.Distinct().ToList()
            });


        }

        public Q OrHavingIn<T>(string column, IEnumerable<T> values)
        {
            return instance.Or().HavingIn(column, values);
        }

        public Q HavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return instance.Not().HavingIn(column, values);
        }

        public Q OrHavingNotIn<T>(string column, IEnumerable<T> values)
        {
            return instance.Or().Not().HavingIn(column, values);
        }


        public Q HavingIn(string column, Q factory)
        {
            return instance.AddComponent("having", new InQueryCondition<Q>
            {
                Column = column,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
                Query = factory,
            });
        }
        public Q HavingIn(string column, Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewQuery());

            return instance.HavingIn(column, query);
        }

        public Q OrHavingIn(string column, Q factory)
        {
            return instance.Or().HavingIn(column, factory);
        }

        public Q OrHavingIn(string column, Func<Q, Q> callback)
        {
            return instance.Or().HavingIn(column, callback);
        }
        public Q HavingNotIn(string column, Q factory)
        {
            return instance.Not().HavingIn(column, factory);
        }

        public Q HavingNotIn(string column, Func<Q, Q> callback)
        {
            return instance.Not().HavingIn(column, callback);
        }

        public Q OrHavingNotIn(string column, Q factory)
        {
            return instance.Or().Not().HavingIn(column, factory);
        }

        public Q OrHavingNotIn(string column, Func<Q, Q> callback)
        {
            return instance.Or().Not().HavingIn(column, callback);
        }


        /// <summary>
        /// Perform a sub factory Having clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Having(string column, string op, Func<Q, Q> callback)
        {
            var query = callback.Invoke(instance.NewChild());

            return instance.Having(column, op, query);
        }

        public Q Having(string column, string op, Q factory)
        {
            return instance.AddComponent("having", new QueryCondition<Q>
            {
                Column = column,
                Operator = op,
                Query = factory,
                IsNot = instance.GetNot(),
                IsOr = instance.GetOr(),
            });
        }

        public Q OrHaving(string column, string op, Q factory)
        {
            return instance.Or().Having(column, op, factory);
        }
        public Q OrHaving(string column, string op, Func<Q, Q> callback)
        {
            return instance.Or().Having(column, op, callback);
        }

        public Q HavingExists(Q factory)
        {
            if (!factory.HasComponent("from"))
            {
                throw new ArgumentException("'FromClause' cannot be empty if used inside a 'HavingExists' condition");
            }

            // simplify the factory as much as possible
            factory = factory.Clone().ClearComponent("select")
                .SelectRaw("1")
                .Limit(1);

            return instance.AddComponent("having", new ExistsCondition<Q>
            {
                Query = factory,
                IsNot = instance.GetNot(),
                IsOr = instance.GetOr(),
            });
        }
        public Q HavingExists(Func<Q, Q> callback)
        {
            var childQuery = NewQuery().SetParent(instance);
            return instance.HavingExists(callback.Invoke(childQuery));
        }

        public Q HavingNotExists(Q factory)
        {
            return instance.Not().HavingExists(factory);
        }

        public Q HavingNotExists(Func<Q, Q> callback)
        {
            return instance.Not().HavingExists(callback);
        }

        public Q OrHavingExists(Q factory)
        {
            return instance.Or().HavingExists(factory);
        }
        public Q OrHavingExists(Func<Q, Q> callback)
        {
            return instance.Or().HavingExists(callback);
        }
        public Q OrHavingNotExists(Q factory)
        {
            return instance.Or().Not().HavingExists(factory);
        }
        public Q OrHavingNotExists(Func<Q, Q> callback)
        {
            return instance.Or().Not().HavingExists(callback);
        }

        #region date
        public Q HavingDatePart(string part, string column, string op, object value)
        {
            return instance.AddComponent("having", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = instance.GetOr(),
                IsNot = instance.GetNot(),
            });
        }
        public Q HavingNotDatePart(string part, string column, string op, object value)
        {
            return instance.Not().HavingDatePart(part, column, op, value);
        }

        public Q OrHavingDatePart(string part, string column, string op, object value)
        {
            return instance.Or().HavingDatePart(part, column, op, value);
        }

        public Q OrHavingNotDatePart(string part, string column, string op, object value)
        {
            return instance.Or().Not().HavingDatePart(part, column, op, value);
        }

        public Q HavingDate(string column, string op, object value)
        {
            return instance.HavingDatePart("date", column, op, value);
        }
        public Q HavingNotDate(string column, string op, object value)
        {
            return instance.Not().HavingDate(column, op, value);
        }
        public Q OrHavingDate(string column, string op, object value)
        {
            return instance.Or().HavingDate(column, op, value);
        }
        public Q OrHavingNotDate(string column, string op, object value)
        {
            return instance.Or().Not().HavingDate(column, op, value);
        }

        public Q HavingTime(string column, string op, object value)
        {
            return instance.HavingDatePart("time", column, op, value);
        }
        public Q HavingNotTime(string column, string op, object value)
        {
            return instance.Not().HavingTime(column, op, value);
        }
        public Q OrHavingTime(string column, string op, object value)
        {
            return instance.Or().HavingTime(column, op, value);
        }
        public Q OrHavingNotTime(string column, string op, object value)
        {
            return instance.Or().Not().HavingTime(column, op, value);
        }

        public Q HavingDatePart(string part, string column, object value)
        {
            return instance.HavingDatePart(part, column, "=", value);
        }
        public Q HavingNotDatePart(string part, string column, object value)
        {
            return instance.HavingNotDatePart(part, column, "=", value);
        }

        public Q OrHavingDatePart(string part, string column, object value)
        {
            return instance.OrHavingDatePart(part, column, "=", value);
        }

        public Q OrHavingNotDatePart(string part, string column, object value)
        {
            return instance.OrHavingNotDatePart(part, column, "=", value);
        }

        public Q HavingDate(string column, object value)
        {
            return instance.HavingDate(column, "=", value);
        }
        public Q HavingNotDate(string column, object value)
        {
            return instance.HavingNotDate(column, "=", value);
        }
        public Q OrHavingDate(string column, object value)
        {
            return instance.OrHavingDate(column, "=", value);
        }
        public Q OrHavingNotDate(string column, object value)
        {
            return instance.OrHavingNotDate(column, "=", value);
        }

        public Q HavingTime(string column, object value)
        {
            return instance.HavingTime(column, "=", value);
        }
        public Q HavingNotTime(string column, object value)
        {
            return instance.HavingNotTime(column, "=", value);
        }
        public Q OrHavingTime(string column, object value)
        {
            return instance.OrHavingTime(column, "=", value);
        }
        public Q OrHavingNotTime(string column, object value)
        {
            return instance.OrHavingNotTime(column, "=", value);
        }

        #endregion
    }
}