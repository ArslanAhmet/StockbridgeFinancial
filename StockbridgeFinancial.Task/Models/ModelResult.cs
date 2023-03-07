using System;
using System.Collections.Generic;
using System.Text;

namespace CefSharp.MinimalExample.OffScreen.Models
{
    public class ModelResult
    {
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public VehicleDetail SpecificVehicle { get; set; }
        public List<Vehicle> VehiclesWithHomeDelivery { get; set; } = new List<Vehicle>();
    }
}
