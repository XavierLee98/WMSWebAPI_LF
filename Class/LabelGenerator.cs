using DbClass;
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZXing.QrCode;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;

namespace WMSWebAPI.Class
{
    public class LabelGenerator : IDisposable
    {
        public void Dispose() => GC.Collect();
        public string LastErrorMessage { get; set; } = string.Empty;

        private IWebHostEnvironment _env;
        readonly string saveLabelFoder = "Labels";

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="env"></param>
        public LabelGenerator(IWebHostEnvironment env) => _env = env;

        /// <summary>
        ///  code referenc from 
        ///  https://social.technet.microsoft.com/wiki/contents/articles/37921.asp-net-core-qr-code-generator-using-zxing-net.aspx
        /// </summary>
        /// <param name="dataValue"></param>
        /// <param name="altValue"></param>
        /// <param name="savePath"></param>
        /// <param name="widthValue"></param>
        /// <param name="heightValue"></param>
        /// <param name="marginValue"></param>
        /// <returns></returns>
        public string GenerateQrCode(string dataValue, string fileName,
            int widthValue = 250, int heightValue = 250, int marginValue = 0)
        {
            try
            {                
                var width = widthValue; // width of the Qr Code   
                var height = heightValue; // height of the Qr Code   
                var margin = marginValue;
                var qrCodeWriter = new ZXing.BarcodeWriterPixelData
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = margin
                    }
                };

                var pixelData = qrCodeWriter.Write(dataValue);
                using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
                using (var ms = new MemoryStream())
                {
                    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), 
                        ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                    try
                    {
                        // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image   
                        Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    // prepare the save file path                    
                    var directoryPath = Path.Combine(_env.WebRootPath, saveLabelFoder, "qr");

                    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
                    var filePath = Path.Combine(directoryPath, fileName);
                    bitmap.Save(filePath, ImageFormat.Png);

                    var test2 = @"http://192.168.137.1:20719/" + saveLabelFoder + "/qr/" + fileName; 
                    
                    return test2;
                }
            }
            catch (Exception excep)
            {
                Console.WriteLine(excep.ToString());
                LastErrorMessage = excep.Message;
                return string.Empty;
            }
        }

        // Suppose this method is responsible for fetching image path
        //public string GetImage()
        //{
        //    var path = Path.Combine(_env.WebRootPath, ChampionsImageFolder);
        //    return path;
        //}
    }
}
