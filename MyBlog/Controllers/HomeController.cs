using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using MyBlog.Models;
using System.Diagnostics;
using System.IO;

namespace MyBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IHostingEnvironment environment;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }       

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        public async Task Upload(IFormFileCollection files)
        {
            string uploadUrl = "http://13.124.189.61/img";
            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    foreach(var file in files)
                    {
                        using (var fileStream = file.OpenReadStream())
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var fileContent = new StreamContent(fileStream);

                            // 파일 컨텐츠를 MultipartFormDataContent에 추가
                            form.Add(fileContent, "file", fileName);

                            // 서버에 요청을 보내고 응답을 받음
                            var response = await httpClient.PostAsync(uploadUrl, form);

                            // 응답 처리
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("파일 업로드 성공");
                            }
                            else
                            {
                                Console.WriteLine("파일 업로드 실패");
                            }

                            //using (Stream fileStream = fileInfo.OpenRead())
                            //{
                            //    // 스트림 이동
                            //    fileStream.CopyTo(stream);
                            //}
                            // stream을 byte로 변환할 버퍼 생성
                            //var ret = new byte[stream.Length];
                            //// 스트림 seek 이동
                            //stream.Seek(0, SeekOrigin.Begin);
                            //// stream을 byte로 이동
                            //stream.Read(ret, 0, ret.Length);




                            HttpClient client = new HttpClient();
                            HttpResponseMessage response2 = await client.GetAsync(uploadUrl);
                            byte[] responseContent = await response2.Content.ReadAsByteArrayAsync();
                            //await System.IO.File.Create(responseContent, "C:\img\mask.png");
                            //await System.IO.File.WriteAllBytesAsync(@"C:\img\mask.png", responseContent);
                        }

                    }
                    
                }
            }
        }





        //public IActionResult Upload(ProductModel model, IFormFileCollection files)
        //{
        //    try
        //    {

        //        foreach (var file in files)
        //        {
        //            if (file.Length > 0)
        //            {

        //                string fileName = Path.GetFileName(Convert.ToString(file.FileName));
        //                // 경로에 파일이 없으면 생성
        //                //string path = "./test";
        //                string path = "./" + model.RegisterUserId;
        //                if (!Directory.Exists(path))
        //                {
        //                    Directory.CreateDirectory(path);
        //                }
        //                string filePath = Path.Combine(path, fileName);
        //                using (var fileStream = new FileStream(filePath, FileMode.Create))
        //                {
        //                    file.CopyTo(fileStream);
        //                }
        //            }
        //        }
        //        return Redirect("/home");
        //    }
        //    catch (Exception ex)
        //    {
        //        // 파일 업로드 실패 처리
        //        Console.WriteLine(ex.Message);
        //        return Redirect("/home");
        //    }
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}