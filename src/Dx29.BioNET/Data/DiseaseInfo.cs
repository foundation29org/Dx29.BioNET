using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Dx29.Data
{
    public class DiseaseInfo
    {
        public DiseaseInfo()
        {
            Symptoms = new Dictionary<string, SymptomInfo>();
            Genes = new Dictionary<string, GeneInfo>();
        }
        public DiseaseInfo(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        [JsonIgnore]
        public int Position { get; set; }

        public double Score { get; set; }
        public double PhenotypeScore => Symptoms?.Count > 0 ? Symptoms.Values.Sum(r => r.Score) : 0.0;
        public double VariantScore => Genes?.Count > 0 ? Genes.Values.Max(r => r.VariantScore) : 0.0;

        public IDictionary<string, SymptomInfo> Symptoms { get; set; }
        public IDictionary<string, GeneInfo> Genes { get; set; }
    }
}
