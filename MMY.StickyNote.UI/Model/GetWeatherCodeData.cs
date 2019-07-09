using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMY.StickyNote.UI
{
    public class GetWeatherCodeData
    {
        public List<CityCode> 城市代码 { get; set; }
    }

    public class CityCode
    {
        public string 省 { get; set; }
        public List<City> 市 { get; set; }
    }
    public class City
    {
        public string 市名 { get; set; }
        public string 编码 { get; set; }
    }
}
