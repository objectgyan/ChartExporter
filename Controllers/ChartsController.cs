using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using Newtonsoft.Json;

namespace ChartExporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChartsController : ControllerBase
    {
        private const string CHROME_PATH = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";

        [HttpGet]
        [Route("export")]
        public async Task<IActionResult> ExportChartImage()
        {
            // Create chart data
            var chartData = new
            {
                labels = new[] { "January", "February", "March", "April", "May", "June", "July" },
                datasets = new[]
                {
            new
            {
                label = "Sales",
                data = new[] { 65, 59, 80, 81, 56, 55, 40 },
                backgroundColor = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#00cc99", "#99ff99", "#d27979", "#fa8072" },
                borderColor = new[] { "#FF6384", "#36A2EB", "#FFCE56", "#00cc99", "#99ff99", "#d27979", "#fa8072" },
                borderWidth = 1
            }
        }
            };

            // Create chart options
            var chartOptions = new
            {
                responsive = true,
                maintainAspectRatio = false,
                scales = new
                {
                    yAxes = new[]
                    {
                new
                {
                    ticks = new
                    {
                        beginAtZero = true
                    }
                }
            }
                }
            };

            // Render chart in HTML
            var chartHtml = $@"
                            <html>
                            <head>
                                <script src='https://cdn.jsdelivr.net/npm/chart.js'></script>
            
                            </head>
                            <body>
                                <canvas id='myChart' width='800' height='600'></canvas>
                            </body>
                            <footer>
                            <script>
                                    var ctx = document.getElementById('myChart').getContext('2d');
                                    var myChart = new Chart(ctx, {{
                                        type: 'bar',
                                        data: {JsonConvert.SerializeObject(chartData)},
                                        options: {JsonConvert.SerializeObject(chartOptions)}
                                    }});
                                </script>
                            </footer>
                            </html>
                            ";

            // Initialize PuppeteerSharp
            var options = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = CHROME_PATH
            };
            // Launch headless Chrome browser
            var browser = await Puppeteer.LaunchAsync(options);

            // Create a new page
            var page = await browser.NewPageAsync();

            // Navigate to a blank page
            await page.GoToAsync("about:blank");

            // Set the page content to the chart HTML
            await page.SetContentAsync(chartHtml);
            // Wait for the chart to render
            await page.WaitForSelectorAsync("#myChart");

            // Take a screenshot of the chart
            var bytes = await page.ScreenshotDataAsync(new ScreenshotOptions
            {
                FullPage = true,
                Type = ScreenshotType.Png
            });

            // Close the browser
            await browser.CloseAsync();

            // Return image as file
            return File(bytes, "image/png", "chart.png");
        }
    }
}