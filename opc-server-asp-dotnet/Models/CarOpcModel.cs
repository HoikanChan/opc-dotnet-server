using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace opc_server_asp_dotnet.Models
{
    public class CarOpcModel
    {
        public Item CarActivated { get; set; }
        public Item SortActivated { get; set; }
        public Item Miding { get; set; }
        public Item BadCar { get; set; }
        public Item Sorting { get; set; }
        public Item CurrentLocation { get; set; }
        public Item Destination { get; set; }
        public Item Direction { get; set; }
        public Item RunClock { get; set; }
        public Item CarNo { get; set; }
        public Item RemainingClock { get; set; }
        public Item Size { get; set; }
        public Item X { get; set; }
        public Item Y { get; set; }

    }
    public class Item
    {
        public Item(int id)
        {
            this.Id = id;
            Value = "";
        }
        public object Value { get; set; }
        public int Id { get; set; }
    }
}