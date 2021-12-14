using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    public partial class DiagnosisService
    {
        public DiagnosisService(BioEntityService bioEntityService, IOntology ontology)
        {
            BioEntityService = bioEntityService;
            Ontology = ontology;
        }

        public BioEntityService BioEntityService { get; }
        public IOntology Ontology { get; }

        public IDictionary<string, IList<SymptomInfo>> DiseaseSymptoms { get; private set; }
        public IDictionary<string, IList<DiseaseInfo>> SymptomDiseases { get; private set; }

        virtual public int Depth => 2;

        public void Initialize()
        {
            CreateDiseaseSymptoms();
            CreateSymptomDiseases();

            CalculateIC();
            CalculateScores();
        }

        private void CalculateIC()
        {
            double total = DiseaseSymptoms.Count;
            foreach (var disease in DiseaseSymptoms)
            {
                foreach (var symptom in disease.Value)
                {
                    symptom.IC = -Math.Log(SymptomDiseases[symptom.Id].Count / total);
                }
            }
        }

        private void CalculateScores()
        {
            foreach (var disease in Ontology.Diseases.Values)
            {
                var arraySymptoms = DiseaseSymptoms[disease.Id];
                CalculateScores(arraySymptoms);
            }
        }

        public void CreateDiseaseSymptoms()
        {
            DiseaseSymptoms = new Dictionary<string, IList<SymptomInfo>>();
            foreach (var disease in Ontology.Diseases.Values)
            {
                var symptoms = new Dictionary<string, SymptomInfo>();
                AddExactSymptoms(disease, symptoms);
                for (int depth = 1; depth <= Depth; depth++)
                {
                    AddAncestorSymptoms(disease, symptoms, depth);
                }
                var arraySymptoms = symptoms.Values.ToArray();
                DiseaseSymptoms.Add(disease.Id, arraySymptoms);
            }
        }

        private void CreateSymptomDiseases()
        {
            SymptomDiseases = new Dictionary<string, IList<DiseaseInfo>>();
            foreach (var diseaseSymptoms in DiseaseSymptoms)
            {
                var id = diseaseSymptoms.Key;
                var orphanet = Ontology.Diseases[id];
                var disease = new DiseaseInfo(id, orphanet.Name) { Desc = orphanet.Desc, Symptoms = DiseaseSymptoms[id].ToDictionary(r => r.Id) };
                foreach (var symptomInfo in diseaseSymptoms.Value)
                {
                    var diseases = SymptomDiseases.TryGetValue(symptomInfo.Id);
                    if (diseases == null)
                    {
                        diseases = new List<DiseaseInfo>();
                        SymptomDiseases.Add(symptomInfo.Id, diseases);
                    }
                    diseases.Add(disease);
                }
            }
        }

        private void AddExactSymptoms(Disease disease, Dictionary<string, SymptomInfo> symptoms)
        {
            foreach (var symptomAssoc in disease.Symptoms)
            {
                foreach (var term in BioEntityService.GetHpoTerms(symptomAssoc.Id))
                {
                    var symptomInfo = new SymptomInfo(term.Id, term.Name)
                    {
                        Relation = 0,
                        Frequency = symptomAssoc.Type
                    };
                    symptoms[term.Id] = symptomInfo;
                }
            }
        }

        private void AddAncestorSymptoms(Disease disease, Dictionary<string, SymptomInfo> symptoms, int depth)
        {
            foreach (var symptomAssoc in disease.Symptoms)
            {
                foreach (var ancestorId in BioEntityService.GetAncestors(symptomAssoc.Id, depth))
                {
                    var symptomInfo = symptoms.TryGetValue(ancestorId);
                    if (symptomInfo == null)
                    {
                        var ancestor = BioEntityService.Hpo[ancestorId];
                        symptomInfo = new SymptomInfo(ancestor.Id, ancestor.Name)
                        {
                            Relation = depth,
                            Frequency = symptomAssoc.Type,
                            RelatedId = symptomAssoc.Id,
                            RelatedName = symptomAssoc.Name
                        };
                    }
                    symptomInfo.Relation = Math.Min(symptomInfo.Relation, depth);
                    symptomInfo.Frequency = MergeFrequencies(symptomInfo.Frequency, symptomAssoc.Type);
                    symptoms[symptomInfo.Id] = symptomInfo;
                }
            }
        }

        private void CalculateScores(IList<SymptomInfo> symptoms)
        {
            foreach (var symptom in symptoms)
            {
                symptom.Score = CalculateScore(symptom) * symptom.IC;
            }
        }

        virtual protected double CalculateScore(SymptomInfo symptom)
        {
            switch (symptom.Frequency)
            {
                case "HP:0040280": return 100;
                case "HP:0040281": return 90;
                case "HP:0040282": return 80;
                case "HP:0040283": return 50;
                case "HP:0040284": return 30;
                case "HP:0040285": return 0;

                case "HP:9999999": return 75;

                default: return 0;
            }
        }

        private string MergeFrequencies(string frequency1, string frequency2)
        {
            int freq1 = Int32.Parse(frequency1.Substring(3));
            int freq2 = Int32.Parse(frequency2.Substring(3));
            int freq = Math.Min(freq1, freq2);
            if (freq == 9999999)
            {
                return "HP:9999999";
            }
            return $"HP:00{freq}";
        }
    }
}
