using KeyLineTest.Models;
using KeyLineTest.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KeyLineTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Import()
        {

            return View();
        }

        [HttpPost]
        public string Import(HttpPostedFileBase[] files, bool importDB ,string connId)
        {
            ImportService impSrv = new ImportService();
            var details = impSrv.SaveHttpPostedFile(files, "~/UploadFile/");
            details.All(d => { d.ImportDB = importDB; return true; }); //不存到資料庫
            var chartData = impSrv.ExtractFile(details, connId);
            impSrv.CorrectChartData(chartData);
            var jsonStr = JsonConvert.SerializeObject(chartData);
            return jsonStr;
        }

        public string ExtendNode(string queryStr)
        {


            ImportService impSrv = new ImportService();
            var query = impSrv.GetAssociateRecord(queryStr);
            var chartData = impSrv.RecordToChartData(query);
            impSrv.CorrectChartData(chartData);
            var jsonStr = JsonConvert.SerializeObject(chartData);


            return jsonStr;
        }

        public string GetPhoneOwner(string queryStr)
        {
            ImportService impSrv = new ImportService();
            var chartData = impSrv.GetPhoneOwner(queryStr);
            impSrv.CorrectChartData(chartData);
            var jsonStr = JsonConvert.SerializeObject(chartData);
            return jsonStr;
        }

        public string GetPhone(string queryStr)
        {
            ChartData chartData = new ChartData();
            ImportService impSrv = new ImportService();
            impSrv.GetPhone(chartData, queryStr);
            impSrv.CorrectChartData(chartData);
            var jsonStr = JsonConvert.SerializeObject(chartData);
            return jsonStr;
        }
    }
}