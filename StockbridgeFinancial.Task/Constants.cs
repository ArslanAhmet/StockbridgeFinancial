using System;
using System.Collections.Generic;
using System.Text;

namespace CefSharp.MinimalExample.OffScreen
{
    public static class Constants
    {
        public static readonly string VehiclesByBadging = @"(function (){

                        var arr = Array.from(document.getElementsByClassName('vehicle-badging')).map(x => (                             
                                 x.getAttribute('data-override-payload')
                        ));

                        return arr;
                     })();";

        public static readonly string CarDetailScript = @"(function (){

                        let vehicle = CARS['initialActivity']; 

                        return vehicle;
                     })();";

        public static readonly string FilterHomeDeliveryCheck = @"document.querySelector('#mobile_home_delivery_true').checked = true;";

        public static readonly string GoNextPage = @"document.querySelector('#next_paginate').click();";
    }
}
