using Dubonnet.Abstractions;

namespace Dubonnet.Internal
{

    internal class KeyPropertyInfo
    {
        public KeyPropertyInfo(DubonProperty propertyInfo, bool isIdentity)
        {
            DubonProperty = propertyInfo;
            IsIdentity = isIdentity;
        }

        public DubonProperty DubonProperty { get; }

        public bool IsIdentity { get; }
    }

}
