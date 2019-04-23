using Newtonsoft.Json;
using opc_server_asp_dotnet.Models;
using opc_server_asp_dotnet.Services;
using opc_server_asp_dotnet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace opc_server_asp_dotnet.Controllers
{
    public class CarController : ApiController
    {
        CarService service = new CarService();
        OPCMonitor opc = OPCMonitor.Instance;
        // GET: api/Car
        public PaginationModel Get(string ordernumber, string timeFrom, string timeTo, string carId,string packageNumber,string checkNumber, string to, int page = 0)
        {
            return service.GetAll(ordernumber, timeFrom, timeTo, carId, packageNumber, checkNumber, to , page);
        }

        // GET: api/Car/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Car
        public void Post([FromBody]Car car)
        {

        }

        // PUT: api/Car/5
        public void Put(int id, [FromBody]string value)
        {
            opc.WriteCarStatus(id, value);
        }

        // DELETE: api/Car/5
        public void Delete(int id)
        {
        }
    }
}
