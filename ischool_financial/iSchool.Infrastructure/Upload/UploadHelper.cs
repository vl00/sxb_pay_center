using iSchool.Domain.Modles;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;

namespace iSchool.Infrastructure.Upload
{
    public class UploadHelper
    {
        public static string GetFileExtension(Stream fileStream)
        {
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Seek(0, SeekOrigin.Begin);
            return GetFileExtension(bytes);
        }
        public static string GetFileExtension(byte[] fileBytes)
        {
            string Extension = "";
            Dictionary<string, byte[]> ImageHeader = new Dictionary<string, byte[]>();
            ImageHeader.Add(".jpg", new byte[] { 255, 216, 255 });
            ImageHeader.Add(".png", new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            ImageHeader.Add(".gif", new byte[] { 71, 73, 70, 56, 57, 97 });
            foreach (string ext in ImageHeader.Keys)
            {
                byte[] header = ImageHeader[ext];
                byte[] test = new byte[header.Length];
                Array.Copy(fileBytes, 0, test, 0, test.Length);
                bool same = true;
                for (int i = 0; i < test.Length; i++)
                {
                    if (test[i] != header[i])
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                {
                    Extension = ext;
                    break;
                }
            }
            return Extension;
        }

        public static (string url, string compressUrl) TransportFile(byte[] bytes, string FileID, string Extension, string FileName, string url)
        {
            return PostFile(string.Format(url, FileID, FileName + Extension), bytes);
        }
        public static (string url, string compressUrl) TransportFile(byte[] bytes, string FileID, out string Extension, string FileName, string url)
        {
            try
            {
                Extension = GetFileExtension(bytes);
                if (string.IsNullOrEmpty(Extension))
                {
                    return default;
                }
                return PostFile(string.Format(url, FileID, FileName + Extension), bytes);
            }
            catch (Exception ex)
            {
                Extension = ".jpg";
                return default;
            }
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        public static (string url, string compressUrl) TransportFile(IFormFile file, string FileName, string FileID, out string Extension, string url)
        {
            try
            {
                var stream = file.OpenReadStream();
                Extension = GetFileExtension(stream);
                if (string.IsNullOrEmpty(Extension))
                {
                    return default;
                }
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
                return PostFile(string.Format(url, FileID, FileName + Extension), bytes);

            }
            catch (Exception ex)
            {
                Extension = ".jpg";
                return default;
            }
        }

        public static (string url, string compressUrl) TransportImage(IFormFile file, string FileName, string FileID, int width, int height, int x, int y, out string Extension, string url)
        {

            try
            {
                var stream = file.OpenReadStream();
                Extension = GetFileExtension(stream);
                if (string.IsNullOrEmpty(Extension))
                {
                    return default;
                }

                //将生成图片转成流

                //var image = System.Drawing.Image.FromStream(stream);
                //var newImage = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);


                //矩形定义,将要在被截取的图像上要截取的图像区域的左顶点位置和截取的大小
                //Rectangle rectSource = new Rectangle(x, y, width, height);


                //矩形定义,将要把 截取的图像区域 绘制到初始化的位图的位置和大小
                //rectDest说明，将把截取的区域，从位图左顶点开始绘制，绘制截取的区域原来大小
                //Rectangle rectDest = new Rectangle(0, 0, width, height);

                //重新绘制图片
                //using (var g = System.Drawing.Graphics.FromImage(newImage))
                //{
                //    //第一个参数就是加载你要截取的图像对象，第二个和第三个参数及如上所说定义截取和绘制图像过程中的相关属性，第四个属性定义了属性值所使用的度量单位
                //    g.DrawImage(image, rectDest, rectSource, GraphicsUnit.Pixel);
                //}

                //判断图片是否太大
                //太大进行压缩
                //if (newImage.Width > 1130)
                //{
                //    var newW = 1130;
                //    var newH = int.Parse(Math.Round(newImage.Height * (double)newW / newImage.Width).ToString());

                //    Bitmap b = new Bitmap(newW, newH);

                //    using (Graphics g = Graphics.FromImage(b))
                //    {
                //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                //        g.DrawImage(newImage, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, newImage.Width, newImage.Height), GraphicsUnit.Pixel);
                //    }
                //    newImage = b;
                //}
                //path = path + Guid.NewGuid().ToString() + Extension;
                //newImage.Save(path);

                url = string.Format(url, FileID, FileName + Extension) + $"&x={x}&y={y}&w={width}&h={height}";

                byte[] bytes = new byte[stream.Length];

                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);

                return PostFile(string.Format(url, FileID, FileName + Extension), bytes, true);
            }
            catch (Exception ex)
            {
                Extension = ".jpg";
                throw new CustomResponseException(ex.Message, 500);
            }
        }

        public static (string url, string compressUrl) TransportImage(IFormFile file, string FileName, string FileID, out string Extension, string url, string path)
        {
            var stream = file.OpenReadStream();
            Extension = GetFileExtension(stream);
            try
            {
                var fid = Guid.NewGuid().ToString();
                var image = Image.FromStream(stream);
                var oldPath = path + fid + Extension;
                image.Save(oldPath);
                if (image.Width > 1130)
                {
                    var newW = 1130;
                    var newH = int.Parse(Math.Round(image.Height * (double)newW / image.Width).ToString());
                    Bitmap b = new Bitmap(newW, newH);
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                        g.DrawImage(image, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
                    }
                    path = path + Guid.NewGuid().ToString() + Extension;
                    b.Save(path);
                    //将生成图片转成流
                    FileInfo fi = new FileInfo(path);
                    byte[] buff = new byte[fi.Length];

                    FileStream fs = fi.OpenRead();
                    fs.Read(buff, 0, Convert.ToInt32(fs.Length));
                    fs.Close();
                    // 设置当前流的位置为流的开始 
                    stream.Seek(0, SeekOrigin.Begin);
                    return PostFile(string.Format(url, FileID, fid + Extension), buff);
                }

                //将生成图片转成流
                FileInfo oldfi = new FileInfo(oldPath);
                byte[] bytes = new byte[oldfi.Length];

                FileStream oldfs = oldfi.OpenRead();
                oldfs.Read(bytes, 0, Convert.ToInt32(oldfs.Length));
                oldfs.Close();

                //byte[] bytes = new byte[stream.Length];
                //stream.Read(bytes, 0, bytes.Length);

                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
                return PostFile(string.Format(url, FileID, fid + Extension), bytes);
            }
            catch (Exception ex)
            {
                Extension = ".jpg";
                throw new CustomResponseException(ex.Message, 500);
            }
        }

        public static Dictionary<Guid, string> TransportFiles(string fileId, string url, params IFormFile[] files)
        {
            Dictionary<Guid, string> filelist = new Dictionary<Guid, string>();
            try
            {
                foreach (var file in files)
                {
                    var fileName = Guid.NewGuid();
                    var stream = file.OpenReadStream();
                    var extName = Path.GetExtension(file.FileName);
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    // 设置当前流的位置为流的开始 
                    stream.Seek(0, SeekOrigin.Begin);
                    var result = PostFileToHulyega(string.Format(url, fileId, fileName + extName), bytes);
                    if (!string.IsNullOrEmpty(result.cdnurl))
                    {
                        filelist.Add(fileName, result.cdnurl);
                    }
                }
                return filelist;
            }
            catch (Exception ex)
            {
                throw new FnResultException(500, ex.Message, ex);
                //return filelist;
            }
        }

        private static (string url, string compressUrl) PostFile(string url, byte[] data, bool isCut = false)
        {
            return PostFileToHulyega(url, data, isCut);
        }

        public static (string url, string cdnurl) PostFileToHulyega(string url, byte[] postData, bool isCut = false)
        {
            try
            {
                HttpWebRequest HRQ = (HttpWebRequest)WebRequest.Create(url);
                HRQ.Method = "post";
                HRQ.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                HRQ.Timeout = 60000;
                HRQ.ContentLength = postData.Length;
                Stream sr_s = HRQ.GetRequestStream();
                sr_s.Write(postData, 0, postData.Length);
                HttpWebResponse RES = (HttpWebResponse)HRQ.GetResponse();
                if (HRQ.HaveResponse)
                {
                    Stream Rs = RES.GetResponseStream();
                    StreamReader RsRead = new StreamReader(Rs);
                    var jk = JToken.Parse(RsRead.ReadToEnd());
                    RsRead.Close();
                    if (isCut)
                    {
                        return (int?)jk["status"] != 0 ? (string.Intern(""), string.Intern("")) : (jk["cdnUrl"].ToString(), jk["compress"]["cdnUrl"].ToString());
                    }
                    return (int?)jk["status"] != 0 ? (string.Intern(""), string.Intern("")) : (jk["url"].ToString(), jk["cdnUrl"].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static List<(string fileName, string videoUrl, string cover, string compressCoverUrl)> PostVideoToHulyega(string fileId, string url, params IFormFile[] files)
        {
            List<(string fileName, string videoUrl, string cover, string compressCoverUrl)> result = new List<(string fileName, string videoUrl, string cover, string compressCoverUrl)>();

            foreach (var file in files)
            {
                var fileName = Guid.NewGuid();
                var stream = file.OpenReadStream();
                var extName = Path.GetExtension(file.FileName);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始 
                stream.Seek(0, SeekOrigin.Begin);
                var item = PostVideoToHulyega(string.Format(url, fileId, fileName + extName), bytes);
                if (!string.IsNullOrEmpty(item.videoUrl))
                    result.Add((fileName.ToString(), item.videoUrl, item.cover, item.compressCoverUrl));
            }
            return result;
        }

        public static (string videoUrl, string cover, string compressCoverUrl) PostVideoToHulyega(string url, byte[] postData)
        {
            try
            {
                HttpWebRequest HRQ = (HttpWebRequest)WebRequest.Create(url);
                HRQ.Method = "post";
                HRQ.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
                HRQ.Timeout = 60000;
                HRQ.ContentLength = postData.Length;
                Stream sr_s = HRQ.GetRequestStream();
                sr_s.Write(postData, 0, postData.Length);
                HttpWebResponse RES = (HttpWebResponse)HRQ.GetResponse();
                if (HRQ.HaveResponse)
                {
                    Stream Rs = RES.GetResponseStream();
                    StreamReader RsRead = new StreamReader(Rs);
                    var jk = JToken.Parse(RsRead.ReadToEnd());
                    RsRead.Close();
                    return (int?)jk["status"] != 0 ? (string.Intern(""), string.Intern(""), string.Intern("")) : (jk["cdnUrl"].ToString(), jk["cover"]["url"].ToString(), jk["cover"]["cdnUrl"].ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return default;
        }
    }
}
