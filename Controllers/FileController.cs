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

            TimeSpan MaxWindow = new TimeSpan();
            double prevPriceForTimeSlot = 0;
            foreach (var row in GridModel.GridData)
            {
                if (DateTime.TryParse(row.Date, out DateTime Temp))
                {
                    var timeSlot = Temp.TimeOfDay;
                    double totalPriceForTimeSlot = 0;
                    foreach (var item in GridModel.GridData.Where(x => DateTime.TryParse(x.Date, out DateTime Temp2) == true && DateTime.Parse(x.Date).TimeOfDay == timeSlot))
                    {
                        totalPriceForTimeSlot += Convert.ToDouble(item.Price.ToString(), CultureInfo.InvariantCulture);
                    }
                    if (totalPriceForTimeSlot > prevPriceForTimeSlot)
                    {
                        MaxWindow = timeSlot;
                    }
                    prevPriceForTimeSlot = totalPriceForTimeSlot;
                }
            }
            GridModel.Calculations.MostExpWindow = MaxWindow.ToString();

            TimeSpan time = TimeSpan.FromHours(1);
            TimeSpan suffix = MaxWindow.Add(time);
            if (suffix.Days == 1)
                suffix = suffix.Subtract(TimeSpan.FromDays(1));
            GridModel.Calculations.MostExpWindowSuffix = suffix.ToString();
        }
    }
}