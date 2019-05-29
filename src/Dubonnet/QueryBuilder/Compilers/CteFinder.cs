using System.Collections.Generic;
using Dubonnet.QueryBuilder.Clauses;

namespace Dubonnet.QueryBuilder.Compilers
{
    public class CteFinder<Q> where Q: QueryFactory<Q>
    {
        private readonly Q _factory;
        private readonly string engineCode;
        private HashSet<string> namesOfPreviousCtes;
        private List<AbstractFrom> orderedCteList;
        
        public CteFinder(Q factory, string engineCode)
        {
            this._factory = factory;
            this.engineCode = engineCode;
        }

        public List<AbstractFrom> Find()
        {
            if (null != orderedCteList)
                return orderedCteList;

            namesOfPreviousCtes = new HashSet<string>();
            
            orderedCteList = FindInternal(_factory);
            
            namesOfPreviousCtes.Clear();
            namesOfPreviousCtes = null;

            return orderedCteList;
        }

        private List<AbstractFrom> FindInternal(Q factoryToSearch)
        {
            var cteList = factoryToSearch.GetComponents<AbstractFrom>("cte", engineCode);

            var resultList = new List<AbstractFrom>();
            
            foreach (var cte in cteList)
            {
                if (namesOfPreviousCtes.Contains(cte.Alias))
                    continue;

                namesOfPreviousCtes.Add(cte.Alias);
                resultList.Add(cte);

                if (cte is QueryFromClause<Q> queryFromClause)
                {
                    resultList.InsertRange(0, FindInternal(queryFromClause.Query));
                }
            }

            return resultList;
        }
    }
}
