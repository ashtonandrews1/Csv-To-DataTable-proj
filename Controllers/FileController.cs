using Microsoft.AspNetCore.Mvc;
using PortalProject.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Globalization;

namespace PortalProject.Controllers
{
    public class FileController : Controller
    {
        [HttpGet]
        public IActionResult Index(GridModelList model = null)
        {
            model = model == null ? new GridModelList() : model;
            return View("~/Views/File/Index.cshtml", model);
        }

        [HttpPost]
        public IActionResult SetGridData(IFormFile file, [FromServices] IWebHostEnvironment hostingEnvironment)
        {
            string fileName = GetFileName(file, hostingEnvironment);
            GridModelList GridModel = GetItemList(fileName);
            SetCalculations(GridModel);

            return View("~/Views/File/Index.cshtml", GridModel);
        }

        private GridModelList GetItemList(string fName)
        {
            GridModelList glm = new GridModelList();
            List<GridModel> items = new List<GridModel>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using (FileStream fileStream = System.IO.File.Open(fName, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(fileStream))
                {
                    while (reader.Read())
                    {
                        if (reader.GetValue(0).ToString() != "Date" && reader.GetValue(1).ToString() != "Market Price EX1")
                        {
                            items.Add(new GridModel()
                            {
                                Date = reader.GetValue(0).ToString(),
                                Price = reader.GetValue(1).ToString(),
                            });
                        }
                    };
                }
            }
            glm.GridData = items;
            return glm;
        }

        private string GetFileName(IFormFile file, IWebHostEnvironment hostingEnvironment)
        {
            string fileName = $"{hostingEnvironment.WebRootPath}\\files\\{file.FileName}";
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }

            return fileName;
        }

        private void SetCalculations(GridModelList GridModel)
        {
            GridModel.Calculations = new GridCalculations();
            GridModel.Calculations.Min = GridModel.GridData.Min(x => Convert.ToDouble(x.Price.ToString(), CultureInfo.InvariantCulture)).ToString();
            GridModel.Calculations.Max = GridModel.GridData.Max(x => Convert.ToDouble(x.Price.ToString(), CultureInfo.InvariantCulture)).ToString();
            GridModel.Calculations.Avg = GridModel.GridData.Average(x => Convert.ToDouble(x.Price.ToString(), CultureInfo.InvariantCulture)).ToString();
            //GridModel.Calculations.MostExpWindow = GridModel.GridData.Average(x => decimal.Parse(x.Price)).ToString();
        }
    }
}