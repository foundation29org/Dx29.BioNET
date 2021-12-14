using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class DiagnosisService
    {
        public DiagnosisResults Predict(string hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            return Predict(hpos.Split(',').Select(r => r.Trim().ToUpper()).ToArray(), genes, skip, count);
        }
        public DiagnosisResults Predict(IList<string> hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            hpos = CurateSymptoms(hpos);

            var diseaseInfos = new Dictionary<string, DiseaseInfo>();

            // Add diseases by genes
            if (genes != null)
            {
                foreach (var geneScore in genes)
                {
                    var gene = Ontology.Genes.TryGetValue(geneScore.Key?.ToUpper());
                    if (gene != null)
                    {
                        foreach (var disease in gene.Diseases)
                        {
                            var result = diseaseInfos.TryGetValue(disease.Id);
                            if (result == null)
                            {
                                result = new DiseaseInfo(disease.Id, disease.Name) { Desc = disease.Name };
                                diseaseInfos.Add(result.Id, result);
                            }
                            var geneInfo = result.Genes.TryGetValue(gene.Id);
                            if (geneInfo == null)
                            {
                                geneInfo = new GeneInfo(gene.Id, gene.Name) { VariantScore = geneScore.Value };
                                result.Genes.Add(gene.Id, geneInfo);
                            }
                            result.Genes[gene.Id].VariantScore = Math.Max(result.Genes[gene.Id].VariantScore, geneScore.Value);
                        }
                    }
                }
            }

            // Add diseases by symptoms
            foreach (var hpo in hpos)
            {
                var diseases = SymptomDiseases.TryGetValue(hpo);
                if (diseases != null)
                {
                    foreach (var disease in diseases)
                    {
                        var result = diseaseInfos.TryGetValue(disease.Id);
                        if (result == null)
                        {
                            result = new DiseaseInfo(disease.Id, disease.Name) { Desc = disease.Desc };
                            diseaseInfos.Add(result.Id, result);
                        }
                        result.Symptoms.Add(hpo, disease.Symptoms[hpo]);
                    }
                }
            }

            var results = diseaseInfos.Values.ToArray();
            var bestTotal = GetBestTotal(results);
            //results = results.Take(bestTotal).ToArray();

            NormalizeScores(results, genes?.Count > 0);

            results = results.OrderBy(r => Ontology.Diseases[r.Id].Symptoms.Count).OrderByDescending(r => r.Score).Skip(skip).Take(count).ToArray();
            return new DiagnosisResults
            {
                Diseases = results,
                Total = diseaseInfos.Count,
                Count = results.Count(),
                BestTotal = bestTotal
            };
        }

        private IList<string> CurateSymptoms(IList<string> hpos)
        {
            var symptoms = new HashSet<string>();
            foreach (var hpo in hpos)
            {
                foreach (var term in BioEntityService.GetHpoTerms(hpo))
                {
                    symptoms.Add(term.Id);
                }
            }
            return symptoms.ToArray();
        }

        private void NormalizeScores(IList<DiseaseInfo> results, bool usingGenes)
        {
            if (results.Count > 0)
            {
                double maxPhenotypeScore = Math.Max(1e-5, results.Max(r => r.PhenotypeScore));
                if (usingGenes)
                {
                    double maxVariantScore = Math.Max(1e-5, results.Max(r => r.VariantScore));
                    foreach (var disease in results)
                    {
                        var normPhenScore = disease.PhenotypeScore / maxPhenotypeScore;
                        var normVarScore = disease.VariantScore / maxVariantScore;
                        //disease.Score = CalcAverage(normPhenScore, normVarScore);
                        disease.Score = CalcRegression(normPhenScore, normVarScore);
                    }
                }
                else
                {
                    foreach (var disease in results)
                    {
                        var normPhenScore = disease.PhenotypeScore / maxPhenotypeScore;
                        disease.Score = Math.Round(normPhenScore, 4);
                    }
                }
            }
        }

        private double CalcAverage(double normPhenScore, double normVarScore)
        {
            return Math.Round((normPhenScore + normVarScore) / 2.0, 2);
        }

        private double CalcRegression(double normPhenScore, double normVarScore)
        {
            double exponent = -(-13.28813 + 10.39451 * normPhenScore + 9.18381 * normVarScore);
            return 1.0 / (1.0 + Math.Exp(exponent));
        }

        private int GetBestTotal(IList<DiseaseInfo> results)
        {
            var matches = results.Select(r => r.Symptoms.Count).ToHashSet();
            if (matches.Count < 2)
            {
                return results.Count;
            }
            int threshold = matches.OrderByDescending(r => r).Take(3).Last();
            return results.Where(r => r.Symptoms.Count >= threshold).Count();
        }
    }
}
