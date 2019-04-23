using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace opc_server_asp_dotnet.Util
{
    public class PaginationModel
    {
        public int DataCount { get; set; } //总记录数
        public int PageCount { get; set; } //总页数
        public int PageNo { get; set; } //当前页码
        public int PageSize { get; set; } //每页显示记录数
        public Object Data { get; set; } //每页显示记录数

    }
}