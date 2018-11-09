using Dubonnet.Abstractions;

namespace Dubonnet.Internal
{
    internal class ForeignKeyInfo
    {
        public ForeignKeyInfo(DubonProperty propertyInfo, ForeignKeyRelation relation)
        {
            DubonProperty = propertyInfo;
            Relation = relation;
        }

        public DubonProperty DubonProperty { get; set; }

        public ForeignKeyRelation Relation { get; set; }
    }


}

