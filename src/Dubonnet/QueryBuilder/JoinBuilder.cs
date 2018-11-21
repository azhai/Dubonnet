using System;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder
{
    public class JoinBuilder<Q> : QueryBase<JoinBuilder<Q>>
    {
        protected string _type = "inner join";

        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value.ToUpper();
            }
        }

        public JoinBuilder() : base()
        {
        }

        public override JoinBuilder<Q> Clone()
        {
            var clone = base.Clone();
            clone._type = _type;
            return clone;
        }

        public JoinBuilder<Q> AsType(string type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Alias for "from" operator.
        /// Since "from" does not sound well with join clauses
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public JoinBuilder<Q> JoinWith(string table) => From(table);
        public JoinBuilder<Q> JoinWith(JoinBuilder<Q> factory) => From(factory);
        public JoinBuilder<Q> JoinWith(Func<JoinBuilder<Q>, JoinBuilder<Q>> callback) => From(callback);

        public JoinBuilder<Q> AsInner() => AsType("inner join");
        public JoinBuilder<Q> AsOuter() => AsType("outer join");
        public JoinBuilder<Q> AsLeft() => AsType("left join");
        public JoinBuilder<Q> AsRight() => AsType("right join");
        public JoinBuilder<Q> AsCross() => AsType("cross join");

        public JoinBuilder<Q> On(string first, string second, string op = "=")
        {
            return AddComponent("where", new TwoColumnsCondition
            {
                First = first,
                Second = second,
                Operator = op,
                IsOr = GetOr(),
                IsNot = GetNot()
            });

        }

        public JoinBuilder<Q> OrOn(string first, string second, string op = "=")
        {
            return Or().On(first, second, op);
        }

        public override JoinBuilder<Q> NewQuery()
        {
            return new JoinBuilder<Q>();
        }

        public override string GetAlias()
        {
            return "";
        }
    }
}