using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public partial class Gene
    {
        public Gene()
        {
            Diseases = new List<Association>();
            Symptoms = new List<Association>();
        }
        public Gene(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public List<Association> Diseases { get; set; }
        public List<Association> Symptoms { get; set; }
    }
}
