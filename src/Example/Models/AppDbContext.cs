using System;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using Dubonnet;
using Dubonnet.Abstractions;

namespace Dubonnet.Example.Models
{
    public class AppDbContext : DubonContext
    {
        public AppDbContext(string dsn)
            : base(new MySqlConnection(dsn), new CustomTableNameResolver())
        {
            InitAllTables();
        }

        public DubonQuery<Area> Areas => InitTable<Area>();
        public DubonQuery<Mobile> Mobiles => InitTable<Mobile>("t_mobile_");
    }
}

public class CustomTableNameResolver : ITableNameResolver
{
    public string Resolve(Type type)
    {
        var lower = type.Name.ToLower();
        return $"t_{lower}s";
    }
}

public class CustomColumnNameResolver : IColumnNameResolver
{
    public string Resolve(DubonProperty info)
    {
        var re = new Regex(@"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])");
        return re.Replace(info.Name, @"$1$3_$2$4").ToLower();
    }
}