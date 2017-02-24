using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace BingWallpaper
{
    internal sealed class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern int SystemParametersInfo(
            int uAction,
            int uParam,
            string lpvParam,
            int fuWinIni);
    }

    class BingWallpaper
    {
        private const string BaseUrl = "http://www.bing.com/HPImageArchive.aspx";

        private const string QueryParameter = "?format=js&idx=0&n=1";

        private const string BingDomain = "http://bing.com";


        public void SetBingWallpaper()
        {
            var imageUrl = GetImageUrl();
            var image = DownloadImage(imageUrl);
            SaveBackgroundImage(image);
            SetBackground();

        }

        private string GetImageUrl()
        {
            var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync(QueryParameter).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            var jsonResult = JObject.Parse(content);

            return GetUrl(jsonResult);
        }

        private string GetUrl(JObject result)
        {
            var urls = result["images"].Select(image => (string) image["url"]).ToList();

            return urls.FirstOrDefault();
        }

        private Image DownloadImage(string imageUrl)
        {
            var client = new HttpClient { BaseAddress = new Uri(BingDomain) };

            var stream = client.GetStreamAsync(imageUrl).Result;
            var background = Image.FromStream(stream);
            return background;
        }

        private void SaveBackgroundImage(Image image)
        {
            image.Save(GetBackgroundPath(), ImageFormat.Jpeg);
        }

        private string GetBackgroundPath()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/BingWallpapers/";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return Path.Combine(directory, DateTime.Now.ToString("M-d-yyyy") + ".Jpeg");
        }

        private void SetBackground()
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            
            key?.SetValue(@"PicturePosition", "10");
            key?.SetValue(@"TileWallpaper", "0");
            
            key?.Close();
            NativeMethods.SystemParametersInfo(20, 0, GetBackgroundPath(), 1 | 2);
        }


    }
}
