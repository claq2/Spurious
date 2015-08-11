using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class Inventory
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public bool IsDead { get; set; }
        public int Quantity { get; set; }
        public override string ToString()
        {
            return string.Format("PID {0} SID {1} Count {2}", ProductId, StoreId, Quantity);
        }
    }
}
