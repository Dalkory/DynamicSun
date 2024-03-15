using DynamicSun.Data;
using DynamicSun.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace DynamicSun.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherDbContext _dbContext = new WeatherDbContext();
        private readonly IWebHostEnvironment _hostEnvironment;

        public WeatherController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index(int month = -1, int year = -1)
        {
            var weatherData = await GetWeatherDataByMonthAndYear(month, year);
            return View(weatherData);
        }

        private async Task<List<WeatherData>> GetWeatherDataByMonthAndYear(int month, int year)
        {
            if (month == -1 && year == -1)
            {
                return new List<WeatherData>();
            }

            var weatherData = await QueryWeatherData(month, year);
            return weatherData;
        }

        private async Task<List<WeatherData>> QueryWeatherData(int month, int year)
        {
            var weatherDataQuery = _dbContext.WeatherData.AsQueryable();

            if (month != -1 && year != -1)
            {
                weatherDataQuery = weatherDataQuery.Where(data => data.Month == month && data.Year == year);
            }
            else if (month != -1)
            {
                weatherDataQuery = weatherDataQuery.Where(data => data.Month == month);
            }
            else if (year != -1)
            {
                weatherDataQuery = weatherDataQuery.Where(data => data.Year == year);
            }

            var weatherData = await weatherDataQuery.ToListAsync();
            return weatherData;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file.FileName.Split(".")[1] != "xlsx")
                {
                    return RedirectToAction("IncorrectFileType");
                }

                var fileName = await SaveFile(file);

                var workbook = await OpenWorkbook(fileName);

                await ProcessSheets(workbook);

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var filePath = Path.Combine(_hostEnvironment.ContentRootPath, file.FileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(fileStream);
            fileStream.Close();

            return filePath;
        }

        private async Task<IWorkbook> OpenWorkbook(string fileName)
        {
            IWorkbook workbook;
            await using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            return workbook;
        }

        private async Task ProcessSheets(IWorkbook workbook)
        {
            for (var sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
            {
                var sheet = workbook.GetSheetAt(sheetIndex);

                var monthData = await ProcessRows(sheet);

                await _dbContext.WeatherData.AddRangeAsync(monthData);
            }
        }

        private async Task<List<WeatherData>> ProcessRows(ISheet sheet)
        {
            var monthData = new List<WeatherData>();

            for (var rowIndex = 4; rowIndex < sheet.LastRowNum + 1; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                var weatherData = await ParseWeatherData(row);

                monthData.Add(weatherData);
            }

            return monthData;
        }

        private async Task<WeatherData> ParseWeatherData(IRow row)
        {
            var day = int.Parse(row.GetCell(0).StringCellValue.Split(".")[0]);
            var month = int.Parse(row.GetCell(0).StringCellValue.Split(".")[1]);
            var year = int.Parse(row.GetCell(0).StringCellValue.Split(".")[2]);

            var tempDate = row.GetCell(0).StringCellValue.Split(".");
            var tempTime = row.GetCell(1).StringCellValue.Split(":");
            var dateAndTime = new DateTimeOffset(int.Parse(tempDate[2]), int.Parse(tempDate[1]), int.Parse(tempDate[0]),
                int.Parse(tempTime[0]), int.Parse(tempTime[1]), 00, TimeSpan.Zero);
            var time = dateAndTime.TimeOfDay.ToString();

            var temperature = row.GetCell(2).NumericCellValue;
            var relativeHumidity = row.GetCell(3).NumericCellValue;
            var temperatureDew = row.GetCell(4).NumericCellValue;
            var atmosphericPressure = int.Parse($"{row.GetCell(5).NumericCellValue}");

            var windDirection = await ParseCellStringValue(row.GetCell(6));
            var windSpeed = await ParseCellNumericValue(row.GetCell(7));
            var cloudCover = await ParseCellNumericValue(row.GetCell(8));
            var cloudBase = await ParseCellNumericValue(row.GetCell(9));
            var visibility = await ParseCellNumericValue(row.GetCell(10));
            var weatherPhenomena = await ParseCellStringValue(row.GetCell(11));

            return new WeatherData
            {
                Day = day,
                Month = month,
                Year = year,
                Time = time,
                Temperature = temperature,
                RelativeHumidity = relativeHumidity,
                TemperatureDew = temperatureDew,
                AtmosphericPressure = atmosphericPressure,
                WindDirection = windDirection,
                WindSpeed = windSpeed,
                CloudCover = cloudCover,
                CloudBase = cloudBase,
                Visibility = visibility,
                WeatherPhenomena = weatherPhenomena,
            };
        }

        private async Task<string?> ParseCellStringValue(ICell cell)
        {
            try
            {
                var value = cell.StringCellValue;
                return string.IsNullOrEmpty(value) ? null : value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<int?> ParseCellNumericValue(ICell cell)
        {
            try
            {
                var value = cell.NumericCellValue;
                return int.Parse(value.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public IActionResult IncorrectFileType()
        {
            return View("IncorrectFileType");
        }

        public IActionResult IncorrectFileData()
        {
            return View("IncorrectFileData");
        }
    }
}