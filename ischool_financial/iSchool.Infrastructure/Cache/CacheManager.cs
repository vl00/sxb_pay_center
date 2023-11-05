using Enyim.Caching;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace iSchool.Infrastructure.Cache
{
    public class CacheManager
    {
        //默认缓存6小时
        public static int _seconds = 6 * 3600;
        public static int Light = 10 * 60;
        public static int Mid = 2 * 3600;
        public static int Deep = 7 * 24 * 3600;
        public static bool memcached = true;

        public readonly string prefixkey;

        private IMemcachedClient _memcachedClient;

        public CacheManager(IMemcachedClient memcachedClient, string prefixkey)
        {
            _memcachedClient = memcachedClient;
            this.prefixkey = prefixkey;
        }


        /// <summary>
        /// 获取缓数据
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public object Get(string cacheKey)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            //var list = new List<KeyValueDto>();
            //list.Add(new KeyValueDto { Key = "test", Value = DateTime.Now, Description = "test" });
            //var data = _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheKey, list, new TimeSpan(0, 0, _seconds));
            var result = _memcachedClient.Get(cacheKey);

            return result;
        }

        public T Get<T>(string cacheKey)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            var result = _memcachedClient.Get<T>(cacheKey);
            return result;
        }
        /// <summary>
        /// 获取string mecached 的值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public string GetStr(string cacheKey)
        {
            var result = Get<string>(cacheKey);
            return result;
        }

        /// <summary>
        /// 获取value跟cas值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public CasResult<T> GetCasResult<T>(string cacheKey)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            var restlt = _memcachedClient.GetWithCas<T>(cacheKey);
            return restlt;
        }


        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Add(string cacheKey, object data)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            return _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheKey, data, new TimeSpan(0, 0, _seconds));
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public bool Remove(string cacheKey)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            return _memcachedClient.Remove(cacheKey);
        }

        /// <summary>
        /// 更改缓存
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="data"></param>
        public void Update(string cacheKey, object data)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            _memcachedClient.Remove(cacheKey);
            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheKey, data, new TimeSpan(0, 0, _seconds));
        }

        public void AddStr(string cacheKey, string data, int seconds = 6 * 3600)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            _memcachedClient.Remove(cacheKey);
            //byte[] bytedata = System.Text.Encoding.ASCII.GetBytes(data);
            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheKey, data, TimeSpan.FromSeconds(seconds));
        }

        public void AddStr(string cacheKey, string data, TimeSpan timeSpan)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            _memcachedClient.Remove(cacheKey);
            //byte[] bytedata = System.Text.Encoding.ASCII.GetBytes(data);
            _memcachedClient.Store(Enyim.Caching.Memcached.StoreMode.Set, cacheKey, data, timeSpan);
        }

        /// <summary>
        /// cas set
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="data"></param>
        /// <param name="timeSpan"></param>
        /// <param name="cas"></param>
        /// <returns></returns>
        public bool CasSet(string cacheKey, object data, TimeSpan timeSpan, ulong cas)
        {
            cacheKey = StrToMD5(prefixkey + cacheKey);
            var casResult = _memcachedClient.Cas(StoreMode.Set, cacheKey, data, timeSpan, cas);
            return casResult.Result;
        }

        /// <summary>
        /// 转换成md5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StrToMD5(string str)
        {
            byte[] data = Encoding.GetEncoding("UTF-8").GetBytes(str + ",;]Sd&@Hhib!$f#^vdv^82%%7(Q&*)#E");
            System.Security.Cryptography.MD5 md5 = new MD5CryptoServiceProvider();
            byte[] OutBytes = md5.ComputeHash(data);

            string OutString = "";
            for (int i = 0; i < OutBytes.Length; i++)
            {
                OutString += OutBytes[i].ToString("x2");
            }
            return OutString.ToUpper();
        }
    }
}
