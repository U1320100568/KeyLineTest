using KeyLineTest.Helpers;
using KeyLineTest.Models;
using KeyLineTest.Models.Repository;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace KeyLineTest.Services
{
    public class ImportService
    {
        private Repository<Record, int> recRep = new Repository<Record, int>();
        private Repository<Person, int> peoRep = new Repository<Person, int>();
        private Repository<Case, int> caseRep = new Repository<Case, int>();
        private Repository<Relationship, int> relRep = new Repository<Relationship, int>();


        //excel存到本機的資料夾
        public List<FileDetail> SaveHttpPostedFile(HttpPostedFileBase[] files, string storePath)
        {
            List<FileDetail> details = new List<FileDetail>();
            foreach (var file in files)
            {
                if (!String.IsNullOrEmpty(file.FileName) && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fileExtend = Path.GetExtension(file.FileName);
                    var path = Path.Combine(HttpContext.Current.Server.MapPath(storePath), fileName);

                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    }

                    ////檢查檔名是否重複，若有重複檔名後加上(2)，以此類推編號
                    int DuplicateNum = 0;
                    string fileNameTmp = fileName;
                    while (System.IO.File.Exists(path))
                    {
                        DuplicateNum++;

                        fileNameTmp = fileName.Replace(fileExtend, "") + "(" + DuplicateNum + ")" + fileExtend;
                        path = Path.Combine(HttpContext.Current.Server.MapPath(storePath), fileNameTmp);
                    }
                    fileName = fileNameTmp;
                    file.SaveAs(path);


                    FileDetail fileDetail = new FileDetail()
                    {
                        FileName = fileName,
                        OriginFileName = file.FileName,
                        PhysicalPath = path,
                        VirtualPath = storePath,
                        Extend = fileExtend
                    };
                    details.Add(fileDetail);
                }


            }
            return details;
        }
        
        //取出excel資料
        public ChartData ExtractFile(List<FileDetail> details, string connId)
        {
            ChartData chartData = new ChartData();
            string fileName = "";

            if(details != null) {

                CalculateFileSize(details);

                var task = Task.Factory.StartNew(() =>
                {
                    foreach (var detail in details)
                    {
                        FileStream fs = null;
                        IWorkbook workbook = null;
                        ISheet sheet = null;

                        chartData.Type = "LinkChart";
                        ImportHelper.KeyLineNodeID = 1;
                        fileName = detail.OriginFileName;

                        DataFormatter format = new DataFormatter(CultureInfo.CurrentCulture);

                        try
                        {
                            using (fs = File.OpenRead(detail.PhysicalPath))
                            {
                                if (detail.Extend == ".xlsx")
                                {
                                    workbook = new XSSFWorkbook(fs);
                                }
                                ///舊excel
                                if (detail.Extend == ".xls")
                                {
                                    workbook = new HSSFWorkbook(fs);
                                }

                                if (workbook != null)
                                {
                                    sheet = workbook.GetSheetAt(0);
                                    if (sheet != null && sheet.LastRowNum > 0)
                                    {

                                        //找first row，並依照關鍵字ImportHelper.FileTypeList() 找資料類型
                                        FindRow(sheet, detail);
                                        ImportHelper.Count += detail.StartRow;
                                        for (int i = detail.StartRow; i <= sheet.LastRowNum; i++)
                                        {
                                            NodePair pair = null;
                                            ImportHelper.Count++;
                                            IRow row = sheet.GetRow(i);
                                            string node1ID ,node2ID, linkTxt;
                                            switch (detail.FileType)
                                            {
                                                case "電話通聯":
                                                    node1ID = format.FormatCellValue(row.GetCell(1));
                                                    node2ID = format.FormatCellValue(row.GetCell(2));
                                                    pair = MapNodePair(node1ID, node2ID);
                                                    
                                                    pair.Node1.Data.Category = "phone";
                                                    pair.Node2.Data.Category = "phone";
                                                    pair.Link.Text = "1";
                                                    pair.Link.Arrow2 = true;
                                                    pair.Link.Data.LinkDatas.Add(new LinkData()
                                                    {
                                                        DateTime = format.FormatCellValue(row.GetCell(3)),
                                                        Period = format.FormatCellValue(row.GetCell(4)),
                                                        ComType = format.FormatCellValue(row.GetCell(0)),
                                                        IMEI = format.FormatCellValue(row.GetCell(5)),
                                                        BaseStation = format.FormatCellValue(row.GetCell(7))
                                                    });

                                                    //存到資料庫
                                                    if (detail.ImportDB)
                                                    {
                                                        SaveToDatabase(sheet.GetRow(i));
                                                    }
                                                    break;
                                                case "成員名冊":
                                                    node1ID = format.FormatCellValue(row.GetCell(0));
                                                    node2ID = format.FormatCellValue(row.GetCell(1));
                                                    pair = MapNodePair(node1ID, node2ID);
                                                    pair.Link.Text = "";
                                                    break;
                                                default:
                                                    break;
                                            }


                                            if (pair != null)
                                            {
                                                AddNodeLink(chartData, pair);
                                            }
                                        }
                                        if (detail.ImportDB)
                                        {
                                            recRep.SaveChanges();
                                        }
                                        
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (fs != null)
                            {
                                fs.Close();
                            }
                        }
                    }
                });

                float ratio = 0;
                Action UpdateProgress = () =>
                {
                    if (ImportHelper.Total != 0)
                    {
                        ratio = (float)((double)ImportHelper.Count / (double)ImportHelper.Total);
                        FileUploaderHub.UpdateProcess(connId, fileName, ratio * 100);
                    }
                };

                while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted)
                {
                    UpdateProgress();
                    Thread.Sleep(200);
                }

                UpdateProgress();

                if(task.IsFaulted)
                {
                    FileUploaderHub.UpdateProcess(connId, "-", 0, string.Join("|",task.Exception.InnerExceptions.Select(o => o.Message).ToArray()));
                }
            }

            //FileUploaderHub.UpdateProcess(connId, "HAHA", 0, "-");
            return chartData;
        }

        //存到資料庫
        public bool SaveToDatabase(IRow row)
        {
            DataFormatter format = new DataFormatter(CultureInfo.CurrentCulture);
            bool parse = DateTime.TryParseExact(format.FormatCellValue(row.GetCell(3)), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime datetime);
            //var test = DateTime.TryParse(format.FormatCellValue(row.GetCell(3)), out DateTime test2);
            Record record = new Record()
            {
                Target = format.FormatCellValue(row.GetCell(1)),
                Opponent = format.FormatCellValue(row.GetCell(2)),
                DateTime = parse ? datetime : (DateTime?) null,
                Period = format.FormatCellValue(row.GetCell(4)),
                ComType = format.FormatCellValue(row.GetCell(0)),
                IMEI = format.FormatCellValue(row.GetCell(5)),
                BaseStation = format.FormatCellValue(row.GetCell(7))

            };

            //確認有無儲存過
            var recordTmp = recRep.Query(r => r.Target == record.Target && r.Opponent == record.Opponent && r.DateTime == record.DateTime).FirstOrDefault();
            if(recordTmp == null)
            {
                recRep.Insert(record);
                
                return true;
            }
            else
            {
                return false;
            }
}
        

        //計算上傳檔案大小
        public void CalculateFileSize(List<FileDetail> details)
        {
            ImportHelper.Total = 0;
            ImportHelper.Count = 0;
            
            foreach (var item in details)
            {
                
                    IWorkbook workbook = null;
                    
                    using (FileStream fs = File.OpenRead(item.PhysicalPath))
                    {
                        if (item.Extend == ".xlsx")
                        {
                            workbook = new XSSFWorkbook(fs);
                        }
                        ///舊excel
                        if (item.Extend == ".xls")
                        {
                            workbook = new HSSFWorkbook(fs);
                        }
                        if (workbook != null)
                        {
                            for (int s = 0; s < workbook.NumberOfSheets; s++)
                            {
                                int lastRowNum = workbook.GetSheetAt(s).LastRowNum != 0 ? workbook.GetSheetAt(s).LastRowNum + 1 : 0;
                                ImportHelper.Total += lastRowNum;  //signalR檔案大小
                            }
                        }
                    }
                
            }
        }

        //尋找資料第一行
        public bool FindRow(ISheet sheet ,FileDetail fileDetail) {

            fileDetail.StartRow = 0;
            
            foreach ( IRow row in sheet)
            {
                foreach(ICell col in row)
                {
                    string cellContent = col.RichStringCellValue.String.Trim();
                    if (ImportHelper.FileTypeList().Values.Contains(cellContent))
                    {
                        fileDetail.FileType = ImportHelper.FileTypeList().FirstOrDefault(v => v.Value == cellContent).Key;
                        fileDetail.StartRow = row.RowNum;
                        
                        if(fileDetail.FileType == "電話通聯")
                        {
                            fileDetail.StartRow++;
                        }

                        return true;
                    }
                }


                
            }
            return false;
        }

        //產生一對node 
        public NodePair MapNodePair(string node1ID, string node2ID) {
            
            NodePair pair = new NodePair() ;
            pair.Node1.ID = node1ID;
            pair.Node1.Text = node1ID;
            pair.Node2.ID = node2ID;
            pair.Node2.Text = node2ID;
            pair.Link.ID = node1ID + "-" + node2ID;

            return pair;
        }
        //增加點和線到 chart data
        public void AddNodeLink(ChartData chartData, NodePair pair)
        {
            //確認線不存在，新增node 和 link
            bool ExistLinkFlag = CheckExistedLink(chartData, pair.Link);
            if (ExistLinkFlag)
            {
                //string imageUrl = "../Image/person.png";

                Color color = Color.FromArgb(ImportHelper.RandColor.Next(256), ImportHelper.RandColor.Next(170,256), ImportHelper.RandColor.Next(200, 256));
                string colorStr = "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");

                //確認點不存在，新增node
                if (!CheckExistedNode(chartData, pair.Node1.ID))
                {
                    pair.Node1.Color = colorStr;
                    chartData.Items.Add(pair.Node1);
                }

                //確認點不存在，新增node
                if (!CheckExistedNode(chartData, pair.Node2.ID))
                {
                    
                    pair.Node2.Color = colorStr;
                    chartData.Items.Add(pair.Node2);
                }


                //加入線
                pair.Link.Color = colorStr;
                pair.Link.ID1 = pair.Node1.ID;
                pair.Link.ID2 = pair.Node2.ID;
                chartData.Items.Add(pair.Link);
            }

            

        }
        
        //檢查同樣的點
        public bool CheckExistedNode(ChartData chartData, string text)
        {
           return  chartData.Items.OfType<Node>().Any(n => n.Text == text);
            
        }
        //檢查同樣的線
        public bool CheckExistedLink(ChartData chartData, Link newLink)
        {
            Link link = chartData.Items.OfType<Link>().FirstOrDefault(k => k.ID == newLink.ID);
            if(link == null)
            {
                return true;
            }
            else
            {
                link.Weight++;
                link.Text = link.Weight.ToString();
                link.Data.LinkDatas.Add(newLink.Data.LinkDatas.FirstOrDefault());
                return false;
            }
        }
        //修正link 粗細
        public void FixLinkWeight(ChartData chartData)
        {
            int maxWeight = chartData.Items.OfType<Link>().Max(l => l.Weight).GetValueOrDefault();
            var modifiedLink = chartData.Items.OfType<Link>()
                .All(l => {
                    l.Weight = l.Weight <= 10 ?
                    l.Weight :
                    Convert.ToInt16(Math.Floor((decimal)l.Weight * 40 / maxWeight));
                    return true;
                    });
            
        }
        //修正chart data的資料
        public void CorrectChartData(ChartData chartData)
        {
            FixLinkWeight(chartData);

            chartData.Items.OfType<Node>().Where(n => n.Data.Category == "phone")
                    .Select(n => { n.FontIcon = new FontIcon("fa-comment-dots"); n.URL = "/Image/phone.png"; return n; }).ToList();

            chartData.Items.OfType<Node>().Where(n => n.Data.Category == "person")
                    .Select(n => {n.FontIcon = new FontIcon("fa-frown-open"); n.URL = "/Image/person.png"; return n; }).ToList();

            chartData.Items.OfType<Node>().Where(n => n.Data.Category == "crime")
                    .Select(n => { n.FontIcon = new FontIcon("fa-address-card"); n.URL = "/Image/crime.png"; return n; }).ToList();

        }

        #region 從資料庫取得關聯點數
        public IQueryable<Record> GetAssociateRecord(string phoneNum)
        {
            Repository<Record, int> rep = new Repository<Record, int>();
            return rep.Query(r => r.Target == phoneNum);
        }

        //取出資料，存到chart data
        public ChartData RecordToChartData(IQueryable<Record> recs)
        {
            ChartData chartData = new ChartData();
            var recList = recs.ToList();
            foreach (var rec in recList)
            {
                var pair = MapNodePair(rec.Target, rec.Opponent);
                pair.Node1.Data.Category = "phone";
                pair.Node2.Data.Category = "phone";
                pair.Link.Text = "1";
                pair.Link.Arrow2 = true;
                pair.Link.Data.LinkDatas.Add(new LinkData()
                {
                    DateTime = rec.DateTime.ToString(),
                    Period = rec.Period,
                    ComType = rec.ComType,
                    IMEI = rec.IMEI,
                    BaseStation = rec.BaseStation
                });

                if (pair != null)
                {
                    AddNodeLink(chartData, pair);
                }

            }
            return chartData;
        }
        #endregion 從資料庫取得關聯點數

        #region 從資料庫取得持有人
        //用電話號碼查持有人
        public ChartData GetPhoneOwner(string phoneNum)
        {
            ChartData chartData = new ChartData();
            var people = peoRep.Query(p => p.Phone == phoneNum).ToList();
            foreach(var person in people)
            {
                var pair = MapNodePair(phoneNum, person.Name);
                pair.Node1.Data.Category = "phone";
                pair.Node2.Data.Category = "person";
                pair.Node2.Data.PersonData.PersonID = person.PersonID.ToString();
                pair.Link.Text = "持有人";
                pair.Link.Arrow = true;
                pair.Link.Arrow2 = true;

                if (pair != null)
                {
                    AddNodeLink(chartData, pair);
                }
                
                GetCrimeRecord(chartData, person.PersonID);
                GetRelationship(chartData);
            }

            
            return chartData;
        }
        //用人 查詢  案件與共犯
        public void GetCrimeRecord(ChartData chartData, int personID)
        {
            NodePair pair = null;
            Person person = peoRep.Find( personID);
            if(person != null)
            {
                List<Case> crimes = caseRep.Query(c => c.PersonID == person.PersonID).ToList();
                foreach(var crime in crimes)
                {
                    pair = MapNodePair(person.Name, crime.Title);
                    pair.Node1.Data.Category = "person";
                    pair.Node2.Data.Category = "crime";
                    pair.Node1.Data.PersonData.PersonID = person.PersonID.ToString();

                    pair.Link.Text = "犯案紀錄";
                    pair.Link.Arrow = true;
                    pair.Link.Arrow2 = true;

                    if (pair != null)
                    {
                        AddNodeLink(chartData, pair);
                    }
                   
                    var relatedIDs = crime.Related.Split(',').ToList();

                    var relateds = peoRep.Query(p => relatedIDs.Contains(p.PersonID.ToString()));
                    foreach(var rel in relateds)
                    {
                        pair = MapNodePair(crime.Title, rel.Name);
                        pair.Node1.Data.Category = "crime";
                        pair.Node2.Data.Category = "person";
                        pair.Node2.Data.PersonData.PersonID = rel.PersonID.ToString();

                        pair.Link.Text = "共犯";
                        pair.Link.Arrow = true;
                        pair.Link.Arrow2 = true;

                        if (pair != null)
                        {
                            AddNodeLink(chartData, pair);
                        }
                        
                    }
                }
            }
        }
        //找尋chart data 有相關的"人"
        public void GetRelationship(ChartData chartData)
        {
            var NodeIDs = chartData.Items.OfType<Node>().Select(n => n.ID);
            var people = peoRep.Query(p => NodeIDs.Contains(p.Name));
            NodePair pair = null;
            foreach(var person in people)
            {
                var rels = relRep.Query(r => r.PersonID1 == person.PersonID|| r.PersonID2 == person.PersonID);
                foreach(var rel in rels)
                {
                    Person related = null;
                    if(rel.PersonID1 != person.PersonID)
                    {
                        related = peoRep.Find(rel.PersonID1);
                    }
                    else
                    {
                        related = peoRep.Find(rel.PersonID2);
                    }

                    pair = MapNodePair(person.Name, related.Name);
                    pair.Node1.Data.Category = "person";
                    pair.Node2.Data.Category = "person";
                    pair.Node1.Data.PersonData.PersonID = person.PersonID.ToString();
                    pair.Node2.Data.PersonData.PersonID = related.PersonID.ToString();

                    pair.Link.Text = rel.Relationship1;
                    pair.Link.Arrow = true;
                    pair.Link.Arrow2 = true;

                    if (pair != null)
                    {
                        AddNodeLink(chartData, pair);
                    }
                    

                    
                }
            }
        }
        //用人找電話
        public void GetPhone(ChartData chartData, string personIDStr)
        {
            //var personID = Convert.ToInt32(personIDStr);
            var person = peoRep.Query(p => p.Name == personIDStr).FirstOrDefault();
            if(person.Phone != null)
            {
                var pair = MapNodePair(person.Name, person.Phone);
                pair.Node1.Data.Category = "person";
                pair.Node2.Data.Category = "phone";
                pair.Link.Text = "持有人";
                pair.Link.Arrow = true;
                pair.Link.Arrow2 = true;

                if (pair != null)
                {
                    AddNodeLink(chartData, pair);
                }
                
            }
        }
        #endregion 從資料庫取得持有人

    }
}