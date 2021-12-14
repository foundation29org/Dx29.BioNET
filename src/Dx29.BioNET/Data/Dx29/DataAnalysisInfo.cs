using System;
using System.Linq;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class DataAnalysisInfo
    {
        public DataAnalysisInfo()
        {
            symptoms = new List<string>();
            genes = new List<DataAnalysisGene>();
        }

        public IList<string> symptoms { get; set; }
        public IList<DataAnalysisGene> genes { get; set; }
    }

    public class DataAnalysisGene
    {
        public DataAnalysisGene()
        {
            diseases = new List<string>();
        }

        public string name { get; set; }
        public double score { get; set; }
        public double? combinedScore { get; set; }
        public IList<string> diseases { get; set; }
    }
}
