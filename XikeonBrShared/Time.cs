using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public static class Time
    {
        public static int h = 12;
        public static int m = 0;
        public static int s = 0;

        public static void Set(int h, int m, int s)
        {
            Time.h = h;
            Time.m = m;
            Time.s = s;
        }
    }
}
