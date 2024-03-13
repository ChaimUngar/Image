using ImageUpload.Data;
using ImageUpload.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Diagnostics;
using System.Text.Json;

namespace ImageUpload.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = @"Data Source=.\sqlexpress; 
                                                        Initial Catalog=ImageUpload;
                                                        Integrated Security=true;";

        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(IFormFile image, string password)
        {
            var fileName = $"{Guid.NewGuid()}-{image.FileName}";
            var fullImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

            using FileStream fs = new FileStream(fullImagePath, FileMode.Create);
            image.CopyTo(fs);

            var repo = new ImageRepository(_connectionString);

            var newImage = new Image
            {
                Password = password,
                ImagePath = fileName
            };
            newImage.Id = repo.Add(newImage);

            return View(newImage);
        }


        public IActionResult ViewImage(Image image, string password)
        {
            var vm = new ImageViewVM();

            List<int> idsViewed = HttpContext.Session.Get<List<int>>("viewed");
            vm.ShowPassword = idsViewed.Count == 0 || !idsViewed.Contains(image.Id);


            var repo = new ImageRepository(_connectionString);
            image = repo.GetById(image.Id);

            if (image.Password != password)
            {
                vm.Message = "Invalid password!!!!";
                return View(image);
            }

            if (image.Password == password && !idsViewed.Contains(image.Id))
            {
                idsViewed.Add(image.Id);
            }

            if (idsViewed.Contains(image.Id))
            {
                repo.IncreaseView(image.Id);
                HttpContext.Session.Set<List<int>>("viewed", idsViewed);
            }

            return View(image);

        }

        //[HttpPost]
        //public IActionResult ViewImage(string password, Image i)
        //{
        //    var repo = new ImageRepository(_connectionString);
        //    i = repo.GetById(i.Id);
        //    var vm = new ImageViewVM();
        //    if (i.Password != password)
        //    {
        //        vm.Message = "Invalid password!!!!";
        //        return RedirectToAction("viewimage");
        //    }

        //    return View();
        //}
    }

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonSerializer.Deserialize<T>(value);
        }
    }
}