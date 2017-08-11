using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortUrl.Model
{
    public class UrlToSave
    {
        public string LongUrl { get; set; }
        public string ShortUrl { get; set; }
        public long Timestamp { get; set; }
    }
}
