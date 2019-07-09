using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMY.StickyNote.UI
{
    public class GetResultData_Items
    {
        public string status { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public AddressComponent addressComponent { get; set; }
    }

    public class AddressComponent
    {

        //"city":"柳州市",
        //"direction":"",
        //"distance":"",
        //"district":"柳北区",
        //"province":"广西壮族自治区",
        //"street":"桃源路",
        //"street_number":""

        public string city { get; set; }
        public string direction { get; set; }
        public string distance { get; set; }
        public string district { get; set; }
        public string province { get; set; }
        public string street { get; set; }
        public string street_number { get; set; }
    }
}
