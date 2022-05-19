using ClosedXML.Excel;
using EShopAdminApp.Models;
using GemBox.Document;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EShopAdminApp.Controllers
{
    public class OrderController : Controller
    {
        public OrderController()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
        }
        public IActionResult Index()
        {
            HttpClient client = new HttpClient();
            string URL = "https://localhost:44300/api/admin/GetAllActiveOrders";
            HttpResponseMessage response = client.GetAsync(URL).Result;
            
            var data = response.Content.ReadAsAsync<List<Order>>().Result;
            
            return View(data);
        }

        public IActionResult GetOrderDetails(int id)
        {
            HttpClient client = new HttpClient();
            string URL = "https://localhost:44300/api/admin/GetOrderDetails";
            var model = new
            {
                Id = id
            };
            //content ni go seralizira modelot so id kako json za da go pratime na api shto treba
            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(URL,content).Result;

            var data = response.Content.ReadAsAsync<Order>().Result; //parsirame samo eden Order

            return View(data);
        }

        public FileResult SavePdf(int id)
        {
            //za da gi dobieme narackite so produktite i nivnite details
            HttpClient client = new HttpClient();
            string URL = "https://localhost:44300/api/admin/GetOrderDetails";
            var model = new
            {
                Id = id
            };
            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(URL, content).Result;

            var result = response.Content.ReadAsAsync<Order>().Result;
            //sledno treba da gootvorime dokumentot, no prvo treba da go iskreirame.
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Invoice.docx");
            var document = DocumentModel.Load(templatePath); //ni go otvara dokumentot 
            document.Content.Replace("{{OrderNumber}}", result.Id.ToString()); //menuvame 
            document.Content.Replace("{{UserName}}", result.OrderBy.Username);
            StringBuilder sb = new StringBuilder();
            double totalPrice = 0.0;
            foreach(var item in result.Products)
            {

                totalPrice += item.Quantity * item.Product.Price;
                sb.AppendLine(item.Product.ProductName + ", quantity: " + item.Quantity+",price: " + item.Product.Price);
                
            }
            document.Content.Replace("{{ProductList}}", sb.ToString());
            document.Content.Replace("{{TotalPrice}}", "$"+ totalPrice.ToString());

            var stream = new MemoryStream(); //od dokumentot ke bide koristen
            //so cel da ako imame 10 lugje so baraat report da se eksportira vo nov pdf file, templatetot da ne se menuva tuku 10 razlicni fajlovi da se napravat
            document.Save(stream, new PdfSaveOptions());



            return File(stream.ToArray(), new PdfSaveOptions().ContentType, "ExportInvocie.pdf");
        }
        
        [HttpGet]
        public IActionResult ExportAllOrders()
        {
            string fileName = "Orders.xlsx";
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            using(var workbook = new XLWorkbook()) //so ova praavime excel dokument so razlicni sheetovi
            {
                IXLWorksheet worksheet= workbook.Worksheets.Add("All Orders"); //imeto na sheetot
                worksheet.Cell(1, 1).Value= "Order Id"; //1redica,1 kolona
                worksheet.Cell(1, 2).Value = "Customer Email";
                //treba da gi dodavame i produktite toa go pravime so api
                HttpClient client = new HttpClient();
                string URL = "https://localhost:44300/api/admin/GetAllActiveOrders";
                HttpResponseMessage response = client.GetAsync(URL).Result;

                var result = response.Content.ReadAsAsync<List<Order>>().Result; //site naracki

                for(int i = 1; i <= result.Count(); i++)
                {
                    var item = result[i-1]; //da pocne od 0 a ne od 1 kako vo forot
                    worksheet.Cell(i + 1, 1).Value = item.Id.ToString();
                    worksheet.Cell(i + 1, 2).Value = item.OrderBy;

                    for(int p = 0; p <item.Products.Count() ; p++)
                    {
                        worksheet.Cell(1, p + 3).Value = "Product " + (p + 1);
                        worksheet.Cell(i + 1, p + 3).Value = item.Products.ElementAt(p).Product.ProductName;
                    }
                }

                //da zacuvame treba
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName); //sodrzina, tip eksel, i ime na file
                }

            }

        }
    }
}
