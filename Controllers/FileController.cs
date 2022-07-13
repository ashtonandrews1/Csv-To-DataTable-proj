using Microsoft.AspNetCore.Mvc;
using PortalProject.Models;
using Microsoft.AspNetCore.Hosting;
using ExcelDataReader;
using System.Threading.Tasks;

namespace PortalProject.Controllers
{
    public class FileController : Controller
    {
        [HttpGet]
        public IActionResult Index(List<MarketItem> list = null)
        {
            list = list == null ? new List<MarketItem>() : list;
            return View(list);
        }

        [HttpPost]
        public IActionResult Index(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            if (file == null)
            {
            }
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }
            var itemList = this.GetItemList(file.FileName);
            return Index(itemList);
        }

        private List<MarketItem> GetItemList(string fName)
        {
            List<MarketItem> items = new List<MarketItem>();
            var fileName = $"{Directory.GetCurrentDirectory()}{@"\wwwroot\files"}" + "\\" + fName;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (FileStream fileStream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(fileStream))
                {
                    while (reader.Read())
                    {
                        items.Add(new MarketItem()
                        {
                            DateString = reader.GetValue(0).ToString(),
                            PriceString = reader.GetValue(1).ToString()
                        });
                    };
                }
            }
            return items;
        }
    }
}