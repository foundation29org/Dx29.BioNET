using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class DiagnosisService
    {
        const int PHRANK_DEPTH = 100;

        public DiagnosisResults Phrank(string hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            return Phrank(hpos.Split(',').Select(r => r.Trim().ToUpper()).ToArray(), genes, skip, count);
        }
        public DiagnosisResults Phrank(IList<string> hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            hpos = CurateSymptoms(hpos);
            hpos = hpos.Union(BioEntityService.GetAncestors(hpos, PHRANK_DEPTH)).ToArray();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Begin timer
            var diseaseScores = new Dictionary<string, double>();
            foreach (var disease in Ontology.Diseases.Values)
            {
                var score = GetPhrankScore(hpos, disease);
                if (score > 0)
                {
                    diseaseScores[disease.Id] = score;
                }
            }
            // End timer
            stopwatch.Stop(); Console.WriteLine(stopwatch.Elapsed.TotalSeconds);

            var diseaseInfos = new Dictionary<string, DiseaseInfo>();
            foreach (var disescore in diseaseScores.OrderByDescending(r => r.Value).Take(count))
            {
                var dise = Ontology.Diseases[disescore.Key];
                var result = new DiseaseInfo(dise.Id, dise.Name) { Desc = dise.Desc, Score = disescore.Value };
                diseaseInfos.Add(result.Id, result);
            }

            return new DiagnosisResults
            {
                Diseases = diseaseInfos.Values.ToArray(),
                Total = diseaseInfos.Count,
                Count = diseaseInfos.Count
            };
        }

        private double GetPhrankScore(IList<string> hpos, Disease disease)
        {
            var dise = disease.Symptoms.Select(r => r.Id).ToArray();
            var diseAnc = dise.Union(BioEntityService.GetAncestors(dise, PHRANK_DEPTH));

            var union = hpos.Intersect(diseAnc).ToArray();

            double score = 0.0;
            foreach (var hpo in union)
            {
                score += GetPhrankICScore(hpo);
            }
            return score;
        }

        private Dictionary<string, double> _phrankScoreCache = new Dictionary<string, double>();

        private double GetPhrankICScore(string hpo)
        {
            if (_phrankScoreCache.TryGetValue(hpo, out double score))
            {
                return score;
            }

            double g = GetSafeDiseases(hpo).Count();
            if (g > 0)
            {
                //double ga = BioEntityService.GetAncestors(hpo, PHRANK_DEPTH).SelectMany(h => GetSafeDiseases(h).Select(r => r.Id)).ToHashSet().Count();
                double ga = BioEntityService.GetAncestors(hpo, PHRANK_DEPTH).SelectMany(h => GetSafeDiseases(h).Select(r => r.Id)).Count();
                if (ga > 0)
                {
                    score = -Math.Log(g / ga, 2);
                }
            }
            _phrankScoreCache[hpo] = score;
            return score;
        }

        private IEnumerable<Association> GetSafeDiseases(string hpo)
        {
            if (Ontology.Symptoms.TryGetValue(hpo, out Symptom symptom))
            {
                return symptom.Diseases;
            }
            return Enumerable.Empty<Association>();
        }
    }
}
