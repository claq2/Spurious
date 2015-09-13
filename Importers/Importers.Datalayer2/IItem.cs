using System;
using System.Collections.Generic;
using System.Linq;

namespace Importers.DataLayer
{
    public interface IItem
    {
        string IdAndDataFieldsAsCsv { get; }
    }
}