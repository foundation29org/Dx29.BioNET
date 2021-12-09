using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dx29.Data
{
    public class Association
    {
        public Association() { }
        public Association(string id, string name, string type, string typeDesc)
        {
            Id = id;
            Name = name;
            Type = type;
            TypeDesc = typeDesc;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeDesc { get; set; }
    }

    public class AssociationComparer : IEqualityComparer<Association>
    {
        public bool Equals(Association x, Association y)
        {
            if (x != null && y != null)
            {
                return x.Id == y.Id;
            }
            return x == y;
        }

        public int GetHashCode([DisallowNull] Association obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
