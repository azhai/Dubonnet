using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Dubonnet.Example.Models;

namespace Dubonnet.Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db;
        private readonly IDatabase redis;

        public HomeController(AppDbContext db, ConnectionMultiplexer redis)
        {
            this.db = db;
            this.redis = redis.GetDatabase();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var reqId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            return View(new ErrorViewModel {RequestId = reqId});
        }

        public IActionResult Index()
        {
            var city = getCityByCode("0755");
            var query = GetMobileQuery("13", "170").Where("city_id", city.id);
            var count = query.CountSharding();
            var mobiles = query.PaginateSharding(1, 10);
            
            ViewData["Phone"] = "区号0755的城市: " + city.ToString();
            ViewData["Message"] = String.Format("符合条件的共有 {0} 个号码，其中前10个是：\n", count);
            foreach (var mob in mobiles)
            {
                ViewData["Message"] += mob.prefix + "\n";
            }

            var key = city.id.ToString();
            redis.StringSet("city:" + key, JsonConvert.SerializeObject(city));
            redis.KeyExpire("city:" + key, TimeSpan.FromSeconds(3600));
            return View();
        }

        public City getCityByCode(string area_code)
        {
            return db.Cities.Where("areacode", area_code).Get();
        }

        public DubonQuery<Mobile> GetMobileQuery(string start, string stop = "")
        {
            var query = db.Mobiles.Where("prefix", ">=", start);
            var startPre = query.CurrentName + (start+"000").Substring(0, 3);
            // query.DbNameMatch = query.GetDbName(true);
            // query.IsTableNameDesc = false;
            if (string.IsNullOrEmpty(stop))
            {
                query.tableFilter = (table, db) => table.CompareTo(startPre) >= 0;
            }
            else
            {
                var stopPre = query.CurrentName + (stop+"999").Substring(0, 3);
                query.Where("prefix", "<=", stop + "9999999");
                query.tableFilter = (table, db) => table.CompareTo(startPre) >= 0
                        && table.CompareTo(stopPre) <= 0;
            }
            return query;
        }
    }
}
