using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KeyLineTest.Models
{
    public class FileDetail
    {
        public string FileName { get; set; }
        public string OriginFileName { get; set; }
        public string PhysicalPath { get; set; }
        public string VirtualPath { get; set; }
        //副檔名
        public string Extend { get; set; }
        //檔案類型: 電話通聯、成員名冊
        public string FileType { get; set; }
        //開始取值列
        public int StartRow { get; set; }
        //是否存資料庫
        public bool ImportDB { get; set; }
    }

    public class ChartData
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        private List<object> _item = new List<object>();
        [JsonProperty("items")]
        public List<object> Items { get { return _item; } set { _item = value; } }

        public ChartData()
        {
            Type = "LinkChart";
        }
    }

    public class Node
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("u")]
        public string URL { get; set; }
        [JsonProperty("t")]
        public string Text { get; set; }
        [JsonProperty("c")]
        public string Color { get; set; }
        [JsonProperty("fi")]
        public FontIcon FontIcon { get; set; }
        //[JsonProperty("x")]
        //public int X { get; set; }
        //[JsonProperty("y")]
        //public int Y { get; set; }
        private CustomData _data = new CustomData();
        [JsonProperty("d")]
        public CustomData Data {
            get { return _data; }
            set {
                _data = value;
            }
        }

        public Node()
        {
            
            Type = "node";
            //URL = imageUrl,
            Color = "#FFFFFF";
            FontIcon = new FontIcon("fa-comment-dots");
        }
    }

    public class Link
    {

        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("t")]
        public string Text { get; set; }
        [JsonProperty("c")]
        public string Color { get; set; }
        [JsonProperty("id1")]
        public string ID1 { get; set; }
        [JsonProperty("id2")]
        public string ID2 { get; set; }
        [JsonProperty("w")]
        public int? Weight { get; set; }
        [JsonProperty("a1")]
        public bool? Arrow { get; set; }
        [JsonProperty("a2")]
        public bool? Arrow2 { get; set; }
        private CustomData _data = new CustomData();
        [JsonProperty("d")]
        public CustomData Data { get {return _data; } set { _data = value; } }

        public Link()
        {
            Type = "link";
            Text = "1";
            Weight = 1;
            Arrow = false;
            Arrow2 = false;
        }
    }

    public class NodePair
    {
        public Node Node1 { get; set; }
        public Node Node2 { get; set; }
        public Link Link { get; set; }
        //public int Direction { get; set; }  // 0: none, 1: link.a1, 2: link.a2
        //public string LinkText { get; set; }
        //public LinkData LinkData { get; set; }

        public NodePair()
        {
            Node1 = new Node();
            Node2 = new Node();
            Link = new Link();
            //Direction = 0;
        }
    }
    
    public class FontIcon
    {
        private string _text = null;
        [JsonProperty("t")]
        public string Text
        {
            get
            {
                return "KeyLines.getFontIcon('" + _text + "')";
            }
            set
            {
                _text = value;
            }
        }

        public FontIcon(string ClassName)
        {
            Text = ClassName; 
            
        }

    }

    public class CustomData
    {
        private List<LinkData> _linkData = new List<LinkData>();
        [JsonProperty("ld")]
        public List<LinkData> LinkDatas { get { return _linkData; } set {_linkData = value; } }
        [JsonProperty("ct")]
        public string Category { get; set; }
        private PersonData _personData = new PersonData();
        [JsonProperty("pd")]
        public PersonData PersonData { get { return _personData; } set{ _personData = value; } }
    }

    public class LinkData
    {
        [JsonProperty("dt")]
        public string DateTime { get; set; }
        [JsonProperty("pr")]
        public string Period { get; set; }
        [JsonProperty("ct")]
        public string ComType { get; set; }
        [JsonProperty("im")]
        public string IMEI { get; set; }
        [JsonProperty("bs")]
        public string BaseStation { get; set; }
    }

    public class PersonData
    {
        [JsonProperty("pid")]
        public string PersonID { get; set; }
    }
}