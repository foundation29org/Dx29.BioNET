using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class DiagnosisService
    {
        public IList<DiffDisease> CalculateDx29(DataAnalysisInfo analysisInfo, int count)
        {
            var genes = analysisInfo.genes == null ? null : analysisInfo.genes.ToDictionary(r => r.name, r => r.score);
            var prediction = Predict(analysisInfo.symptoms, genes, skip: 0, count: count);
            var results = BuildDx29Results(analysisInfo.symptoms, prediction).ToArray();
            return results;
        }

        public IList<DiffDisease> PhrankDx29(DataAnalysisInfo analysisInfo, int count)
        {
            var genes = analysisInfo.genes == null ? null : analysisInfo.genes.ToDictionary(r => r.name, r => r.score);
            var prediction = Phrank(analysisInfo.symptoms, genes, skip: 0, count: count);
            var results = BuildDx29Results(analysisInfo.symptoms, prediction).ToArray();
            return results;
        }

        private IEnumerable<DiffDisease> BuildDx29Results(IList<string> symptoms, DiagnosisResults prediction)
        {
            foreach (var disease in prediction.Diseases)
            {
                yield return BuildDx29Result(symptoms, disease);
            }
        }

        private DiffDisease BuildDx29Result(IList<string> hpos, DiseaseInfo diseaseInfo)
        {
            var diffDisease = new DiffDisease
            {
                Id = diseaseInfo.Id,
                Name = diseaseInfo.Name,
                Desc = diseaseInfo.Desc,
                ScoreSymptoms = (int)(diseaseInfo.Score * 100),
                Type = "disease",
                Genes = new MatchesGenes()
            };

            // Symptom match
            foreach (var hpo in hpos)
            {
                var symptomInfo = diseaseInfo.Symptoms.TryGetValue(hpo);
                if (symptomInfo != null)
                {
                    var symp = new DiffSymptom
                    {
                        Id = hpo,
                        //Frequency = HPOFrequencies[symptomInfo.Frequency],
                        Frequency = new HPOTerm { Id = symptomInfo.Frequency },
                        HasPatient = true,
                        HasDisease = true,
                        RelatedId = symptomInfo.RelatedId,
                        Relationship = symptomInfo.Relation == 0 ? "Equal" : "Predecessor"
                    };
                    diffDisease.Symptoms.Add(symp);
                }
                else
                {
                    var symp = new DiffSymptom
                    {
                        Id = hpo,
                        //Frequency = HPOFrequencies[0],
                        Frequency = new HPOTerm { Id = "HP:9999999" },
                        HasPatient = true,
                        HasDisease = false,
                        Relationship = "None"
                    };
                    diffDisease.Symptoms.Add(symp);
                }
            }

            // Disease match
            var disease = GetDisease(diseaseInfo.Id);
            foreach (var symptom in disease.Symptoms)
            {
                if (!diseaseInfo.Symptoms.Values.Any(r => r.RelatedId == symptom.Id))
                {
                    var symp = new DiffSymptom
                    {
                        Id = symptom.Id,
                        //Frequency = HPOFrequencies[symptom.Type],
                        Frequency = new HPOTerm { Id = symptom.Type },
                        HasPatient = false,
                        HasDisease = true,
                        Relationship = "None"
                    };
                    diffDisease.Symptoms.Add(symp);
                }
            }

            // Add genes
            diffDisease.Genes = new MatchesGenes();
            foreach (var gene in diseaseInfo.Genes)
            {
                diffDisease.Genes.Add(gene.Value.Name, new MatchGene { label = gene.Value.Id });
            }

            diffDisease.Symptoms = diffDisease.Symptoms.OrderBy(r => r.HasPatient).OrderBy(r => r.Frequency.Id).ToList();
            diffDisease.ScoreSymptoms = (int)(diseaseInfo.PhenotypeScore);
            diffDisease.ScoreGenes = (int)(diseaseInfo.VariantScore * 100);
            diffDisease.ScoreDx29 = (int)(diseaseInfo.Score * 100);
            return diffDisease;
        }

        private Disease GetDisease(string id)
        {
            return Ontology.Diseases[id];
        }
    }
}
