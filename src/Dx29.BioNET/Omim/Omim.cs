using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    public class Omim : IOntology
    {
        public Omim(BioEntityService bioEntityService)
        {
            BioEntityService = bioEntityService;

            Diseases = new Dictionary<string, Disease>();
            Symptoms = new Dictionary<string, Symptom>();
            Genes = new Dictionary<string, Gene>();
            Mappings = new Dictionary<string, HashSet<string>>();
        }

        public void Initialize()
        {
            string hpoaFilename = Path.Combine("_data", "phenotype.hpoa");

            LoadSymptoms(hpoaFilename);

            BuildSymptoms();
            DescribeDiseases();
        }

        public string Prefix => "OMIM";

        public BioEntityService BioEntityService { get; }

        public Dictionary<string, Disease> Diseases { get; }
        public Dictionary<string, Symptom> Symptoms { get; }
        public Dictionary<string, Gene> Genes { get; }

        public Dictionary<string, HashSet<string>> Mappings { get; set; }

        private void LoadSymptoms(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("#"))
                    {
                        var record = ParseLine(line);
                        if (record.Id.StartsWith("OMIM:"))
                        {
                            var disease = GetOrCreateDisease(record);
                            AddAssociations(disease, record);
                        }
                    }
                    line = reader.ReadLine();
                }
            }
        }

        private OmimRecord ParseLine(string line)
        {
            var record = new OmimRecord();
            var parts = line.Split('\t');
            record.Id = parts[0];
            record.Name = parts[1];
            record.Hpo = parts[3];
            record.Frequency = parts[7];
            record.Aspect = parts[10];
            return record;
        }

        private void AddAssociations(Disease disease, OmimRecord record)
        {
            if (record.Aspect?.ToUpper() == "P")
            {
                foreach (var term in BioEntityService.GetHpoTerms(record.Hpo))
                {
                    (var type, var typeDesc) = ParseAssocType(record);
                    var association = new Association(term.Id, term.Name, type, typeDesc);
                    disease.Symptoms.Add(association);
                }
            }
        }

        private void BuildSymptoms()
        {
            foreach (var disease in Diseases.Values)
            {
                foreach (var symp in disease.Symptoms)
                {
                    var symptom = GetOrCreateSymptom(symp.Id, symp.Name);
                    var assoc = new Association(disease.Id, disease.Name, symp.Type, symp.TypeDesc);
                    symptom.Diseases.Add(assoc);
                }
            }
        }

        private Disease GetOrCreateDisease(OmimRecord record)
        {
            var id = record.Id;
            if (!Diseases.TryGetValue(id, out Disease disease))
            {
                disease = new Disease(id, record.Name) { Desc = TryGeDiseasetDesc(id) };
                Diseases.Add(id, disease);
            }
            return disease;
        }

        private string TryGeDiseasetDesc(string id)
        {
            var omins = BioEntityService.GetOmimTerms(id);
            foreach (var omin in omins)
            {
                if (!String.IsNullOrEmpty(omin.Desc))
                {
                    return omin.Desc;
                }
            }
            return null;
        }

        private Symptom GetOrCreateSymptom(string id, string name)
        {
            if (!Symptoms.TryGetValue(id, out Symptom symptom))
            {
                symptom = new Symptom(id, name);
                Symptoms.Add(id, symptom);
            }
            return symptom;
        }

        private void DescribeDiseases()
        {
            foreach (var disease in Diseases.Values)
            {
                var mondos = BioEntityService.GetOmimTerms(disease.Id);
                disease.Desc = mondos?.FirstOrDefault()?.Desc;
            }
        }

        private (string, string) ParseAssocType(OmimRecord record)
        {
            switch (record.Frequency?.ToUpper())
            {
                case "HP:0040280":
                    return ("HP:0040280", "Obligate");
                case "HP:0040281":
                    return ("HP:0040281", "Very frequent");
                case "HP:0040282":
                    return ("HP:0040282", "Frequent");
                case "HP:0040283":
                    return ("HP:0040283", "Occasional");
                case "HP:0040284":
                    return ("HP:0040284", "Very rare");
                case "HP:0040285":
                    return ("HP:0040285", "Excluded");
                default:
                    return ("HP:9999999", "Unknown");
            }
        }

        public bool Match(string local, string external)
        {
            if (Mappings.ContainsKey(external))
            {
                return Mappings[external].Contains(local);
            }
            return false;
        }

        class OmimRecord
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Hpo { get; set; }
            public string Frequency { get; set; }
            public string Aspect { get; set; }
        }
    }
}
