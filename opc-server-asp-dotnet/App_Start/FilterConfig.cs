﻿using System.Web;
using System.Web.Mvc;

namespace opc_server_asp_dotnet
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
