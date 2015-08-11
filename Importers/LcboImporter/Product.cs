using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class Product
    {
        public int Id { get; set; }
        public bool IsDead { get; set; }
        public string Name { get; set; }
        public ProductCategtory Category { get; set; }
        public int Volume { get; set; }
        public override string ToString()
        {
            return string.Format("{3} {0} {1} {2}mL", Name, Category, Volume, Id);
        }
    }
}
