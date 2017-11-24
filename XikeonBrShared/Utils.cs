using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XikeonBrShared
{
    public static class Utils
    {
        public static string ExpandoObjectToString(ExpandoObject obj)
        {
            return string.Join("; ", obj.Select(x => x.Key + "=" + x.Value));
        }
    }
}
