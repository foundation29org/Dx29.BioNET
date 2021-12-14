using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public partial class Disease
    {
        public Disease()
        {
            Symptoms = new HashSet<Association>(new AssociationComparer());
            Genes = new HashSet<Association>(new AssociationComparer());
            XRefs = new Dictionary<string, string>();
        }
        public Disease(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public int Type { get; set; }
        public int Group { get; set; }
        public string Parent { get; set; }

        public IList<string> Synonyms { get; set; }

        public HashSet<Association> Symptoms { get; set; }
        public HashSet<Association> Genes { get; set; }
        public IDictionary<string, string> XRefs { get; set; }
    }
}
