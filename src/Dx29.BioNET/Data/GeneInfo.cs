using System;

namespace Dx29.Data
{
    public class GeneInfo
    {
        public GeneInfo()
        {
        }
        public GeneInfo(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public double VariantScore { get; set; }
    }
}
