﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text;
using System.Windows;
using Ivony.Html;
using Ivony.Html.Parser;
using MMY.StickyNote.UI.Common;

namespace MMY.StickyNote.UI
{
    public class WeatherInfo
    {
        GeoCoordinateWatcher watcher;

        Action<string> _SetWeatherControl;
        string _ak = "elxrPCAh6eQyyBwqk62NGpKMxhTh1az1";
        public void GetLocationEvent(Action<string> SetWeatherControl)
        {
            _SetWeatherControl = SetWeatherControl;
            watcher = new GeoCoordinateWatcher();
            watcher.MovementThreshold = 1.0;
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            bool started = watcher.TryStart(false, TimeSpan.FromMilliseconds(2000));
            if (!started)
            {
                MessageBox.Show("GeoCoordinateWatcher 开启超时！", "提示");
            }
        }


        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {

            AddressComponent address = GetGeocoderForPositionByBaidu(e.Position.Location.Latitude, e.Position.Location.Longitude);

            if (address == null) return;
            //获取城市编码
            string cityCode = GetCityCode(address);
            //使用委托的方式回传地址信息回去，再执行获取天气信息
            string weatherStr = RequestWeatherWebAnalysisData(cityCode);
            _SetWeatherControl(weatherStr);

            watcher.PositionChanged -= watcher_PositionChanged;//关闭位置获取

        }

        private AddressComponent GetGeocoderForPositionByBaidu(double latitude, double longitude)
        {
            //Console.WriteLine("Latitude: {0}, Longitude {1}", latitude, longitude);
            //********http://api.map.baidu.com/geocoder?location=24.363,109.402&output=json&key=elxrPCAh6eQyyBwqk62NGpKMxhTh1az1

            string staticAddress = "http://api.map.baidu.com/geocoder?";
            string location = string.Format("{0},{1}", latitude, longitude);

            string url = string.Format("{0}location={1}&output=json&key={2}", staticAddress, location, _ak);

            try
            {
                string result = GenerateUrl.Get(url);
                AddressComponent address = AnalyticalData.AnalyticalData_obj(result);
                Console.WriteLine("百度请求");
                return address;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取具体地址失败，请稍后右键任务栏小图标点击刷新！", "提示");
                return null;
            }


        }
        public string GetCityCode(AddressComponent address)
        {
            WeatherCodeData data = JsonConvert.DeserializeObject<WeatherCodeData>(MMY.StickyNote.UI.Properties.Resources.WeatherCode);
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

        public string RequestWeatherWebAnalysisData(string cityCode)
        {
            //获取天气网址的html页面
            IHtmlDocument source = null;

            try
            {
                source = new JumonyParser().LoadDocument($"http://www.weather.com.cn/weather1d/{cityCode}.shtml", Encoding.GetEncoding("utf-8"));

            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取天气信息失败！，请稍后右键任务栏小图标点击刷新", "提示");
                return null;
            }

            var input = source.Find("input[id=hidden_title]").First();//获取input标签id为hidden_title
            var divs = source.Find("div[class=xyn-weather-box]");//查找城市所在的div
            var span = divs.Find("h2").Find("span").First();
            //城市名
            string cityName = span.InnerText().Trim();
            string weatherInfo = input.Attribute("value").Value();
            //分割天气信息
            string[] weatherStrs = weatherInfo.Split(' ');
            List<string> wList = new List<string>();
            foreach (var itemStr in weatherStrs)
            {
                if (!string.IsNullOrWhiteSpace(itemStr))
                {
                    wList.Add(itemStr);
                }
            }

            return $"{cityName} - {wList[2]} - {wList[3]}";
        }
    }
}
