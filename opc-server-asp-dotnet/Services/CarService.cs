using opc_server_asp_dotnet.Models;
using opc_server_asp_dotnet.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace opc_server_asp_dotnet.Services
{
    public class CarService
    {
        private ServerDbContext db = new ServerDbContext();
        const int OnePageNumber = 10;

        public PaginationModel GetAll(string ordernumber, string timeFrom, string timeTo, string carId, string packageNumber, string checkNumber, string to, int page = 0)
        {

            using (var context = new ServerDbContext())
            {
                var carsFited = context.Cars
                    .Where(car=> string.IsNullOrEmpty(ordernumber) || car.OrderNumber.Contains(ordernumber))
                    .Where(car => string.IsNullOrEmpty(carId) || car.CarId.Contains(carId))
                    .Where(car => string.IsNullOrEmpty(packageNumber)  || car.PackageNumber.Contains(packageNumber))
                    .Where(car => string.IsNullOrEmpty(checkNumber) || car.CheckNumber.Contains(checkNumber))
                    .Where(car => string.IsNullOrEmpty(to) || car.To.Contains(to))
                    .ToList();
                var cars = carsFited.Skip(page * OnePageNumber).Take(OnePageNumber);
                var result = new PaginationModel()
                {
                    Data = cars,
                    DataCount = carsFited.Count,
                    PageCount = carsFited.Count / OnePageNumber,
                    PageSize = OnePageNumber,
                    PageNo = page
                };
                return result;
            }
        }
    }
}