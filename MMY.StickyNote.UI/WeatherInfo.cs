using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MMY.StickyNote.UI
{
    public class WeatherInfo
    {
       
         GeoCoordinateWatcher watcher;
         string _cityCode;

        Action<AddressComponent> _GetAddressCode;
         string _ak = "elxrPCAh6eQyyBwqk62NGpKMxhTh1az1"; 
        public void GetLocationEvent(Action<AddressComponent> GetAddressCode)
        {
            _GetAddressCode = GetAddressCode;
            watcher = new GeoCoordinateWatcher();
            watcher.MovementThreshold = 1.0;
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            bool started = watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
            {
                MessageBox.Show("GeoCoordinateWatcher 开启超时！","提示");
            }
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {

            AddressComponent address = GetGeocoderForPositionByBaidu(e.Position.Location.Latitude , e.Position.Location.Longitude);
            //使用委托的方式回传地址信息回去，再执行获取天气信息
            _GetAddressCode(address);
            
        }

        private AddressComponent GetGeocoderForPositionByBaidu(double latitude , double longitude)
        {
            Console.WriteLine("Latitude: {0}, Longitude {1}", latitude, longitude);
            //********http://api.map.baidu.com/geocoder?location=24.363,109.402&output=json&key=elxrPCAh6eQyyBwqk62NGpKMxhTh1az1

            string staticAddress = "http://api.map.baidu.com/geocoder?";
            string location = String.Format("{0},{1}", latitude, longitude);

            string url = String.Format("{0}location={1}&output=json&key={2}",staticAddress , location , _ak);
            string result = Get(url);
            AddressComponent address = AnalyticalData(result);
            return address; 

        }

        public WeatherInfoData GetWeatherData( string code)
        {
            //***********http://www.weather.com.cn/data/cityinfo/101110101.html
            string staticAddress = "http://www.weather.com.cn/data/cityinfo/";
            string url = String.Format("{0}{1}.html",staticAddress, code);
            string result = Get(url);
            //Console.WriteLine(result);
            GetWheatherLocalData data = JsonConvert.DeserializeObject<GetWheatherLocalData>(result);
            //Console.WriteLine(data.weatherinfo.weather);
            return data.weatherinfo;
        }

        public string GetCityCode(AddressComponent address)
        {
            GetWeatherCodeData data = JsonConvert.DeserializeObject<GetWeatherCodeData>(MMY.StickyNote.UI.Properties.Resources.WeatherCode);
            string tempGetProvince = address.province.Substring(0, 2);
            string tempGetCity = address.city.Substring(0, 2);
            string cityCode = "0";
            //Console.WriteLine(tempGetProvince);
            foreach (var item_p in data.城市代码)
            {
                //Console.WriteLine(item_p.省);
                string tempLocalProvince = item_p.省.Substring(0, 2);
                if (tempGetProvince == tempLocalProvince)
                {
                    foreach (var item_c in item_p.市)
                    {
                        string tempLocalCity = item_c.市名.Substring(0, 2);
                        if (tempGetCity == tempLocalCity)
                        {
                            cityCode = item_c.编码;
                        }
                    }
                }
               
            }
            return cityCode;
        }

        private  string Get(string url)
        {
            string result = string.Empty;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
        }
        private  AddressComponent  AnalyticalData(string jsonData)
        {
            var obj = JsonConvert.DeserializeObject<GetResultData_Items>(jsonData);
            if (obj.status == "OK")
            {
                AddressComponent items = obj.result.addressComponent;
                //Console.WriteLine(items.city);
                return items;
            }
            else
            {
                MessageBox.Show(obj.status + "**未能获取信息！", "请求异常");
                return null;
            }

        }

    }
}
