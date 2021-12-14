using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public partial class Symptom
    {
        public Symptom()
        {
            Diseases = new List<Association>();
            Genes = new List<Association>();
        }
        public Symptom(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public List<Association> Diseases { get; set; }
        public List<Association> Genes { get; set; }
    }
}
