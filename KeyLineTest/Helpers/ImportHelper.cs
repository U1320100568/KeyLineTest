using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeyLineTest.Helpers
{
    public class ImportHelper
    {
        private static Random _randPosition = new Random();

        public static int KeyLineNodeID
        {
            get; set;
        }

        public static int RandPosition
        {
            get
            {
                return _randPosition.Next(500);
            }
        }

        public static Random RandColor = new Random();

        public static IDictionary<string, string> FileTypeList()
        {
            IDictionary<string, string> dic = new Dictionary<string, string>
            {
                { "電話通聯", "目標電話"  },
                { "成員名冊", "大哥" }
            };
            return dic;
        }

        public static int Total{ get; set; }
        public static int Count { get; set; }

    }
}