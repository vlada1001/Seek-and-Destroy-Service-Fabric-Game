using System;

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
    }
}
