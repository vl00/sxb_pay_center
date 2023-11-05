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
        /// �ֲ�ʽ��
        /// </summary>
        /// <param name="key">��key</param>
        /// <param name="lockExpirySeconds">���Զ���ʱʱ��(��)</param>
        /// <param name="waitLockSeconds">�ȴ���ʱ��(��)</param>
        /// <returns></returns>
        public static bool Lock(this CSRedisClient client, string key, int lockExpirySeconds = 10, double waitLockSeconds =10)
        {
            //����ȴ�50����
            int waitIntervalMs = 50;
            string lockKey = "FinanceCenterLock:" + key;
            DateTime begin = DateTime.Now;
            while (true)
            {
                //ѭ����ȡȡ��
                if (client.SetNx(lockKey, new byte[] { 1 }))
                {
                    //�������Ĺ���ʱ��
                    client.Expire(lockKey, lockExpirySeconds);
                    return true;
                }

                //���ȴ����򷵻�
                if (waitLockSeconds == 0) break;

                //�����ȴ�ʱ�䣬���ٵȴ�
                if ((DateTime.Now - begin).TotalSeconds >= waitLockSeconds) break;

                Thread.Sleep(waitIntervalMs);
            }
            return false;

        }

        /// <summary>
        /// redis��
        /// </summary>
        /// <param name="client"></param>
        /// <param name="lockKey">��key</param>
        /// <param name="value">��value</param>
        /// <param name="lockExpirySeconds">���Զ���ʱʱ��(��)</param>
        /// <returns></returns>
        public static bool Lock(this CSRedisClient client, string lockKey,object value, int lockExpirySeconds = 10)
        {
            if (client.SetNx(lockKey, value))
            {
                //�������Ĺ���ʱ��
                client.Expire(lockKey, lockExpirySeconds);
                return true;
            }
            return false;

        }

        /// <summary>
        /// ɾ���� ִ��������Ժ�����ͷ���
        /// </summary>
        /// <param name="key"></param>
        public static void DelLock(this CSRedisClient client, string key)
        {

            string lockKey = "FinanceCenterLock:" + key;

            client.Del(lockKey);

        }
    }
}
