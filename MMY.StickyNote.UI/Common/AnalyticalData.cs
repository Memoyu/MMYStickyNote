using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace MMY.StickyNote.UI.Common
{
    public static class AnalyticalData
    {
        public static AddressComponent AnalyticalData_obj(string jsonData)
        {
            var obj = JsonConvert.DeserializeObject<ResultData_Items>(jsonData);
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
