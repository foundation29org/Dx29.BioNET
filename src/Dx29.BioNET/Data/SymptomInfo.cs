using System;

namespace Dx29.Data
{
    public class SymptomInfo
    {
        public SymptomInfo()
        {
        }
        public SymptomInfo(string id, string name) : this()
        {
            Id = id;
            Name = name;
            RelatedId = Id;
            RelatedName = Name;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        public string RelatedId { get; set; }
        public string RelatedName { get; set; }

        public int Relation { get; set; } // 0 Exact, 1 Parent, 2 ParentParent, ...
        public string Frequency { get; set; }

        public double IC { get; set; }
        public double Score { get; set; }
    }
}
