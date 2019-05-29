using Dubonnet.Attributes;

namespace Dubonnet.Example.Models
{
    public enum IspEnum
    {
        [EnumDesc("未知")] X,
        [EnumDesc("中国电信")] TEL,
        [EnumDesc("中国移动")] MOB,
        [EnumDesc("中国联通")] UNI
    }

    public class Area
    {
        public int id { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string areacode { get; set; }
        public string zipcode { get; set; }
        public int changed_at { get; set; }

        public override string ToString()
        {
            return string.Concat(province, " ", city, " ", areacode);
        }
    }

    public class Mobile
    {
        public int id { get; set; }
        public int area_id { get; set; }
        public string prefix { get; set; }
        public IspEnum ispcode { get; set; }
        public int changed_at { get; set; }
    }
}
