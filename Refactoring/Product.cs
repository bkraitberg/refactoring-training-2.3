﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    [Serializable]
    public class Product
    {
        [JsonProperty("Name")]
        public string Name;
        [JsonProperty("Price")]
        public double Price;
        [JsonProperty("Quantity")]
        public int Qty;
        [JsonProperty("Id")]
        public int Id;

        public bool HasStock()
        {
            return Qty > 0;
        }
    }
}
