using System;

namespace Dx29.Data
{
    public class DiseaseDesc
    {
        public DiseaseDesc()
        {
        }
        public DiseaseDesc(string id, string name) : this()
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }
}
