using CSRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool
{
    public static class CSRedisCoreExtension
    {
        public static Task<object[]> StartPipeAsync(this CSRedisClient redisClient, Action<CSRedisClientPipe<string>> handler, CancellationToken cancellation = default)
        {
            using var pipe = redisClient.StartPipe();
            handler(pipe);
            return pipe.EndPipeAsync(cancellation);
        }

        public static Task<object[]> EndPipeAsync<T>(this CSRedisClientPipe<T> pipe, CancellationToken cancellation = default)
        {
            return Task.Factory.StartNew(o =>
            {
                using var p = (CSRedisClientPipe<T>)o;
                return p.EndPipe();
            }, pipe, cancellation);
        }
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key">锁key</param>
        /// <param name="lockExpirySeconds">锁自动超时时间(秒)</param>
        /// <param name="waitLockSeconds">等待锁时间(秒)</param>
        /// <returns></returns>
        public static bool Lock(this CSRedisClient client, string key, int lockExpirySeconds = 10, double waitLockSeconds =10)
        {
            //间隔等待50毫秒
            int waitIntervalMs = 50;
            string lockKey = "FinanceCenterLock:" + key;
            DateTime begin = DateTime.Now;
            while (true)
            {
                //循环获取取锁
                if (client.SetNx(lockKey, new byte[] { 1 }))
                {
                    //设置锁的过期时间
                    client.Expire(lockKey, lockExpirySeconds);
                    return true;
                }

                //不等待锁则返回
                if (waitLockSeconds == 0) break;

                //超过等待时间，则不再等待
                if ((DateTime.Now - begin).TotalSeconds >= waitLockSeconds) break;

                Thread.Sleep(waitIntervalMs);
            }
            return false;

        }

        /// <summary>
        /// redis锁
        /// </summary>
        /// <param name="client"></param>
        /// <param name="lockKey">锁key</param>
        /// <param name="value">锁value</param>
        /// <param name="lockExpirySeconds">锁自动超时时间(秒)</param>
        /// <returns></returns>
        public static bool Lock(this CSRedisClient client, string lockKey,object value, int lockExpirySeconds = 10)
        {
            if (client.SetNx(lockKey, value))
            {
                //设置锁的过期时间
                client.Expire(lockKey, lockExpirySeconds);
                return true;
            }
            return false;

        }

        /// <summary>
        /// 删除锁 执行完代码以后调用释放锁
        /// </summary>
        /// <param name="key"></param>
        public static void DelLock(this CSRedisClient client, string key)
        {

            string lockKey = "FinanceCenterLock:" + key;

            client.Del(lockKey);

        }
    }
}
