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
            var area = GetAreaByCode("0755");
            var query = GetMobileQuery("13", "170").Where("area_id", area.id);
            var count = query.CountSharding();
            var mobiles = query.PaginateSharding(1, 10);
            
            ViewData["Phone"] = "区号0755的城市: " + area.ToString();
            ViewData["Message"] = String.Format("符合条件的共有 {0} 个号码，其中前10个是：\n", count);
            foreach (var mob in mobiles)
            {
                ViewData["Message"] += mob.prefix + "\n";
            }

            var key = area.id.ToString();
            redis.StringSet("area:" + key, JsonConvert.SerializeObject(area));
            redis.KeyExpire("area:" + key, TimeSpan.FromSeconds(3600));
            return View();
        }

        public Area GetAreaByCode(string area_code)
        {
            return db.Areas.Where("areacode", area_code).Get();
        }

        public DubonQuery<Mobile> GetMobileQuery(string start, string stop = "")
        {
            var prefix = "t_mobile_";
            var startPre = prefix+(start+"000").Substring(0, 3);
            var query = db.Mobiles.Where("prefix", ">=", start);
            if (string.IsNullOrEmpty(stop))
            {
                query.tableFilter = name => name.CompareTo(startPre) >= 0;
            }
            else
            {
                var stopPre = prefix+(stop+"999").Substring(0, 3);
                query.Where("prefix", "<=", stop + "9999999");
                query.tableFilter = name => name.CompareTo(startPre) >= 0
                        && name.CompareTo(stopPre) <= 0;
            }
            return query;
        }
    }
}
