using System;
using System.Collections.Generic;
using System.Linq;

namespace Dx29.Data
{
    public class DiffDisease
    {
        public DiffDisease()
        {
            Symptoms = new List<DiffSymptom>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }

        public int ScoreDx29 { get; set; }
        public int ScoreGenes { get; set; }
        public int ScoreSymptoms { get; set; }

        public string Type { get; set; }
        public List<DiffSymptom> Symptoms { get; set; }

        public MatchesGenes Genes { get; set; }
    }

    public class HPOTerm
    {
        public string Id { get; set; } 
    }

    public class DiffSymptom : HPOTerm
    {
        public HPOTerm Frequency { get; set; }
        public bool HasPatient { get; set; }
        public bool HasDisease { get; set; }
        public string Relationship { get; set; }

        public string RelatedId { get; set; }
    }

    public class ClinicalInfo : HPOTerm
    {
    }

    public class MatchesGenes: Dictionary <string, MatchGene>
    {

    }

    public class MatchGene 
    {
        public string label { get; set; }
    }

    public class XRefs
    {
        public string Reference { get; set; }
        public XRefData Info { get; set; }

    }

    public class XRefData
    {
        public string Id { get; set; }
        public string Link { get; set; }
    }
}
