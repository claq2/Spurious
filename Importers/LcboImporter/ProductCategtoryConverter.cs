using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LcboImporter
{
    public class ProductCategtoryConverter : ITypeConverter
    {
        static readonly Dictionary<string, ProductCategtory> map = new Dictionary<string, ProductCategtory> 
            {
                { "Wine", ProductCategtory.Wine },
                { "Beer", ProductCategtory.Beer },
                { "Ready-to-Drink/Coolers", ProductCategtory.Cooler },
                { "Spirits", ProductCategtory.Spirit },
                { "Ciders", ProductCategtory.Cider },
            };

        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return type == typeof(ProductCategtory);
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            var result = ProductCategtory.Unknown;
            map.TryGetValue(text, out result);
            return result;
        }

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            throw new NotImplementedException();
        }
    }
}
