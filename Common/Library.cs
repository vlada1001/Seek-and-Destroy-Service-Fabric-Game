using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class Library
    {
        public static Status StringToStatus(string state)
        {
            if (state.ToLower().Equals("infight"))
                return Status.InFight;
            else if (state.ToLower().Equals("exploring"))
                return Status.Exploring;
            else
                return Status.Unknown;
        }
        public static string StatusToString(Status state)
        {
            if (state.Equals(Status.InFight))
                return "infight";
            else if (state.Equals(Status.Exploring))
                return "exploring";
            else
                return "unknown";
        }

        public enum Status
        {
            InFight,
            Exploring,
            Unknown
        }

        public static long RandomLong()
        {
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public static double Distance(int x1, int x2, int y1, int y2)
        {
            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public static long GetPartitionKey(Guid key)
        {
            byte[] hash;
            using (MD5 md5 = MD5.Create())
            {
                md5.Initialize();
                md5.ComputeHash(Encoding.UTF8.GetBytes(key.ToString()));
                hash = md5.Hash;
            }
            return BitConverter.ToInt64(hash);
        }

        public static IEnumerable<Tuple<BigInteger, BigInteger>> SplitInterval(Tuple<long, long> range, int n)
        {
            BigInteger diff = BigInteger.Subtract(range.Item2, range.Item1);
            BigInteger w = BigInteger.Divide(diff, n);

            return from i in Enumerable.Range(0, n) select new Tuple<BigInteger, BigInteger>(range.Item1 + i * w, range.Item1 + (i + 1) * w);
        }

        public static List<Int64> GetPartitionsLowKey(int n)
        {
            BigInteger diff = BigInteger.Subtract(Int64.MaxValue, Int64.MinValue);
            BigInteger w = BigInteger.Divide(diff, n);

            List<Int64> res = (from i in Enumerable.Range(0, n) select ((Int64)new BigInteger((Int64)(Int64.MinValue + i * w + 1)))).ToList();
            res[0] -= 1;

            return res;
        }
    }
}
