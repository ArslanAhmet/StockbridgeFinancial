using System;
using System.Collections.Generic;
using System.Text;

namespace CefSharp.MinimalExample.OffScreen.Models
{
    public class Vhr
    {
        public bool accidents_or_damage { get; set; }
        public bool one_owner_vehicle { get; set; }
        public bool open_recall { get; set; }
        public bool personal_use_only { get; set; }
        public string vhr_type { get; set; }
    }
}
