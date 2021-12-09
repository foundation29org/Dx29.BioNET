using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class DiagnosisResults
    {
        public DiagnosisResults()
        {
            Diseases = new List<DiseaseInfo>();
        }

        public IList<DiseaseInfo> Diseases { get; set; }

        public int Count { get; set; }
        public int Total { get; set; }
        public int BestTotal { get; set; }
    }
}
