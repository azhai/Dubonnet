using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public abstract partial class QueryBase<Q>
    {
        public Q Where(string column, string op, object value)
        {
            // If the value is "null", we will just assume the developer wants to add a
            // where null clause to the factory. So, we will allow a short-cut here to
            // that method for convenience so the developer doesn't have to check.
            if (value == null)
            {
                return Not(op != "=").WhereNull(column);
            }

            return AddComponent("where", new BasicCondition
            {
                Column = column,
                Operator = op,
                Value = value,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNot(string column, string op, object value)
        {
            return Not().Where(column, op, value);
        }

        public Q OrWhere(string column, string op, object value)
        {
            return Or().Where(column, op, value);
        }

        public Q OrWhereNot(string column, string op, object value)
        {
            return instance.Or().Not().Where(column, op, value);
        }

        public Q Where(string column, object value)
        {
            return Where(column, "=", value);
        }

        public Q WhereNot(string column, object value)
        {
            return WhereNot(column, "=", value);
        }

        public Q OrWhere(string column, object value)
        {
            return OrWhere(column, "=", value);
        }

        public Q OrWhereNot(string column, object value)
        {
            return OrWhereNot(column, "=", value);
        }

        /// <summary>
        /// Perform a where constraint
        /// </summary>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public Q Where(object constraints)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in constraints.GetType().GetRuntimeProperties())
            {
                dictionary.Add(item.Name, item.GetValue(constraints));
            }

            return Where(dictionary);
        }

        public Q Where(IReadOnlyDictionary<string, object> values)
        {
            var query = instance;
            var orFlag = GetOr();
            var notFlag = GetNot();

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

                query = instance.Not(notFlag).Where(tuple.Key, tuple.Value);
            }

            return query;
        }

        public Q WhereRaw(string sql, params object[] bindings)
        {
            return AddComponent("where", new RawCondition
            {
                Expression = sql,
                Bindings = Helper.Flatten(bindings).ToArray(),
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereRaw(string sql, params object[] bindings)
        {
            return Or().WhereRaw(sql, bindings);
        }

        /// <summary>
        /// Apply a nested where clause
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Where(Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewChild());

            // omit empty queries
            if (!query.Clauses.Where(x => x.Component == "where").Any())
            {
                return instance;
            }

            return AddComponent("where", new NestedCondition<Q>
            {
                Query = query,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public Q WhereNot(Func<Q, Q> callback)
        {
            return Not().Where(callback);
        }

        public Q OrWhere(Func<Q, Q> callback)
        {
            return Or().Where(callback);
        }

        public Q OrWhereNot(Func<Q, Q> callback)
        {
            return Not().Or().Where(callback);
        }

        public Q WhereColumns(string first, string op, string second)
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q OrWhereColumns(string first, string op, string second)
        {
            return Or().WhereColumns(first, op, second);
        }

        public Q WhereNull(string column)
        {
            return AddComponent("where", new NullCondition
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotNull(string column)
        {
            return Not().WhereNull(column);
        }

        public Q OrWhereNull(string column)
        {
            return instance.Or().WhereNull(column);
        }

        public Q OrWhereNotNull(string column)
        {
            return Or().Not().WhereNull(column);
        }

        public Q WhereTrue(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = true,
            });
        }

        public Q OrWhereTrue(string column)
        {
            return Or().WhereTrue(column);
        }

        public Q WhereFalse(string column)
        {
            return AddComponent("where", new BooleanCondition
            {
                Column = column,
                Value = false,
            });
        }

        public Q OrWhereFalse(string column)
        {
            return Or().WhereFalse(column);
        }

        public Q WhereLike(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "like",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotLike(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereLike(column, value, caseSensitive);
        }

        public Q OrWhereLike(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereLike(column, value, caseSensitive);
        }

        public Q OrWhereNotLike(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereLike(column, value, caseSensitive);
        }

        public Q WhereStarts(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "starts",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereStarts(column, value, caseSensitive);
        }

        public Q OrWhereStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereStarts(column, value, caseSensitive);
        }

        public Q OrWhereNotStarts(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereStarts(column, value, caseSensitive);
        }

        public Q WhereEnds(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "ends",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereEnds(column, value, caseSensitive);
        }

        public Q OrWhereEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereEnds(column, value, caseSensitive);
        }

        public Q OrWhereNotEnds(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereEnds(column, value, caseSensitive);
        }

        public Q WhereContains(string column, string value, bool caseSensitive = false)
        {
            return AddComponent("where", new BasicStringCondition
            {
                Operator = "contains",
                Column = column,
                Value = value,
                CaseSensitive = caseSensitive,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotContains(string column, string value, bool caseSensitive = false)
        {
            return Not().WhereContains(column, value, caseSensitive);
        }

        public Q OrWhereContains(string column, string value, bool caseSensitive = false)
        {
            return Or().WhereContains(column, value, caseSensitive);
        }

        public Q OrWhereNotContains(string column, string value, bool caseSensitive = false)
        {
            return Or().Not().WhereContains(column, value, caseSensitive);
        }

        public Q WhereBetween<T>(string column, T lower, T higher)
        {
            return AddComponent("where", new BetweenCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Lower = lower,
                Higher = higher
            });
        }

        public Q OrWhereBetween<T>(string column, T lower, T higher)
        {
            return Or().WhereBetween(column, lower, higher);
        }

        public Q WhereNotBetween<T>(string column, T lower, T higher)
        {
            return Not().WhereBetween(column, lower, higher);
        }

        public Q OrWhereNotBetween<T>(string column, T lower, T higher)
        {
            return Or().Not().WhereBetween(column, lower, higher);
        }

        public Q WhereIn<T>(string column, IEnumerable<T> values)
        {
            // If the developer has passed a string most probably he wants List<string>
            // since string is considered as List<char>
            if (values is string)
            {
                string val = values as string;

                return AddComponent("where", new InCondition<string>
                {
                    Column = column,
                    IsOr = GetOr(),
                    IsNot = GetNot(),
                    Values = new List<string> {val}
                });
            }

            return AddComponent("where", new InCondition<T>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Values = values.Distinct().ToList()
            });
        }

        public Q OrWhereIn<T>(string column, IEnumerable<T> values)
        {
            return Or().WhereIn(column, values);
        }

        public Q WhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Not().WhereIn(column, values);
        }

        public Q OrWhereNotIn<T>(string column, IEnumerable<T> values)
        {
            return Or().Not().WhereIn(column, values);
        }


        public Q WhereIn(string column, Q factory)
        {
            return AddComponent("where", new InQueryCondition<Q>
            {
                Column = column,
                IsOr = GetOr(),
                IsNot = GetNot(),
                Query = factory,
            });
        }

        public Q WhereIn(string column, Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewQuery());

            return WhereIn(column, query);
        }

        public Q OrWhereIn(string column, Q factory)
        {
            return Or().WhereIn(column, factory);
        }

        public Q OrWhereIn(string column, Func<Q, Q> callback)
        {
            return Or().WhereIn(column, callback);
        }

        public Q WhereNotIn(string column, Q factory)
        {
            return Not().WhereIn(column, factory);
        }

        public Q WhereNotIn(string column, Func<Q, Q> callback)
        {
            return Not().WhereIn(column, callback);
        }

        public Q OrWhereNotIn(string column, Q factory)
        {
            return Or().Not().WhereIn(column, factory);
        }

        public Q OrWhereNotIn(string column, Func<Q, Q> callback)
        {
            return Or().Not().WhereIn(column, callback);
        }


        /// <summary>
        /// Perform a sub factory where clause
        /// </summary>
        /// <param name="column"></param>
        /// <param name="op"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Q Where(string column, string op, Func<Q, Q> callback)
        {
            var query = callback.Invoke(NewChild());

            return Where(column, op, query);
        }

        public Q Where(string column, string op, Q factory)
        {
            return AddComponent("where", new QueryCondition<Q>
            {
                Column = column,
                Operator = op,
                Query = factory,
                IsNot = GetNot(),
                IsOr = GetOr(),
            });
        }

        public Q OrWhere(string column, string op, Q factory)
        {
            return Or().Where(column, op, factory);
        }

        public Q OrWhere(string column, string op, Func<Q, Q> callback)
        {
            return Or().Where(column, op, callback);
        }

        #region date

        public Q WhereDatePart(string part, string column, string op, object value)
        {
            return AddComponent("where", new BasicDateCondition
            {
                Operator = op,
                Column = column,
                Value = value,
                Part = part,
                IsOr = GetOr(),
                IsNot = GetNot(),
            });
        }

        public Q WhereNotDatePart(string part, string column, string op, object value)
        {
            return Not().WhereDatePart(part, column, op, value);
        }

        public Q OrWhereDatePart(string part, string column, string op, object value)
        {
            return Or().WhereDatePart(part, column, op, value);
        }

        public Q OrWhereNotDatePart(string part, string column, string op, object value)
        {
            return Or().Not().WhereDatePart(part, column, op, value);
        }

        public Q WhereDate(string column, string op, object value)
        {
            return WhereDatePart("date", column, op, value);
        }

        public Q WhereNotDate(string column, string op, object value)
        {
            return Not().WhereDate(column, op, value);
        }

        public Q OrWhereDate(string column, string op, object value)
        {
            return Or().WhereDate(column, op, value);
        }

        public Q OrWhereNotDate(string column, string op, object value)
        {
            return Or().Not().WhereDate(column, op, value);
        }

        public Q WhereTime(string column, string op, object value)
        {
            return WhereDatePart("time", column, op, value);
        }

        public Q WhereNotTime(string column, string op, object value)
        {
            return Not().WhereTime(column, op, value);
        }

        public Q OrWhereTime(string column, string op, object value)
        {
            return Or().WhereTime(column, op, value);
        }

        public Q OrWhereNotTime(string column, string op, object value)
        {
            return Or().Not().WhereTime(column, op, value);
        }

        public Q WhereDatePart(string part, string column, object value)
        {
            return WhereDatePart(part, column, "=", value);
        }

        public Q WhereNotDatePart(string part, string column, object value)
        {
            return WhereNotDatePart(part, column, "=", value);
        }

        public Q OrWhereDatePart(string part, string column, object value)
        {
            return OrWhereDatePart(part, column, "=", value);
        }

        public Q OrWhereNotDatePart(string part, string column, object value)
        {
            return OrWhereNotDatePart(part, column, "=", value);
        }

        public Q WhereDate(string column, object value)
        {
            return WhereDate(column, "=", value);
        }

        public Q WhereNotDate(string column, object value)
        {
            return WhereNotDate(column, "=", value);
        }

        public Q OrWhereDate(string column, object value)
        {
            return OrWhereDate(column, "=", value);
        }

        public Q OrWhereNotDate(string column, object value)
        {
            return OrWhereNotDate(column, "=", value);
        }

        public Q WhereTime(string column, object value)
        {
            return WhereTime(column, "=", value);
        }

        public Q WhereNotTime(string column, object value)
        {
            return WhereNotTime(column, "=", value);
        }

        public Q OrWhereTime(string column, object value)
        {
            return OrWhereTime(column, "=", value);
        }

        public Q OrWhereNotTime(string column, object value)
        {
            return OrWhereNotTime(column, "=", value);
        }

        #endregion
    }
}