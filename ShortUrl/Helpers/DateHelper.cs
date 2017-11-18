using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortUrl.Helpers
{
    public class DateHelper
    {
        static public DateTime JavaLongToCSharpLong(long javaLong)
        {
            TimeSpan ss = TimeSpan.FromMilliseconds(javaLong * 1000);
            DateTime Jan1st1970 =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime ddd = Jan1st1970.Add(ss);
            DateTime final = ddd.ToUniversalTime();

            return ddd;
        }

        static public long CSharpMillisToJavaLong(DateTime dateTime)
        {
            DateTime Jan1st1970 =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long diff =
                (long)(dateTime.ToUniversalTime() - Jan1st1970)
                .TotalMilliseconds;
            return diff / 1000;
        }
    }
}
