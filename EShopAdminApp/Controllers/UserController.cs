using EShopAdminApp.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
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
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ImportUsers(IFormFile file)
        {
            string pathToUpload = $"{Directory.GetCurrentDirectory()}\\files\\{file.FileName}";
            using(FileStream filestream = System.IO.File.Create(pathToUpload)) //za da go stavime fileot vo patekata
            {
                file.CopyTo(filestream); //se stava fileot 
                filestream.Flush();
            }

            //da ekstrakne poadtocite od excel i da vrati vo lista na useri
            List<User> users = getAllUsersFromFile(file.FileName);

            HttpClient client = new HttpClient();
            string URL = "https://localhost:44300/api/admin/ImportAllUsers";
            HttpContent content = new StringContent(JsonConvert.SerializeObject(users), Encoding.UTF8, "application/json");//ovoj content ke go pratime
            HttpResponseMessage response = client.PostAsync(URL,content).Result; //na toa url shto specificiravme i koj content


            return RedirectToAction("Index", "Order");
        }

        private List<User> getAllUsersFromFile(string fileName)
        {
            string path = $"{Directory.GetCurrentDirectory()}\\files\\{fileName}";
            List<User> users = new List<User>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);//so ova koristime utf encoding
            using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read)) //da ni otvori fajlot od patekata i samo da e readonly
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while(reader.Read())
                    {
                        users.Add(new Models.User
                        {
                            Email = reader.GetValue(0).ToString(),
                            Password = reader.GetValue(1).ToString(),
                            ConfirmPassword = reader.GetValue(2).ToString()

                        }) ;  //ni dava lista od users bazirano na excel fileot
                    }
                }
            }
            return users;

        }
       
    }
}
