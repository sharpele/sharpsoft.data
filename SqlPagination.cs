using System;
using System.Collections.Generic;
using System.Text;

namespace SharpSoft.Data
{
    public class SqlPagination
    {
        public int CurrentPage { get; set; }

        public int PageCount { get; set; }

        public object PageData { get; set; }
    }
}
