using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    public partial class OrphaNET : IOntology
    {
        public OrphaNET()
        {
            Diseases = new Dictionary<string, Disease>();
            Symptoms = new Dictionary<string, Symptom>();
            Genes = new Dictionary<string, Gene>();

            Mappings = new Dictionary<string, HashSet<string>>();
            MappingsReverse = new Dictionary<string, HashSet<string>>();
        }

        public void Initialize(string filename = null)
        {
            if (filename == null)
            {
                LoadORDO();
            }
            else
            {
                LoadORDO(filename);
            }

            string phensFilename = Path.Combine("_data", "orpha-phen.xml");
            string genesFilename = Path.Combine("_data", "orpha-gene.xml");

            LoadSymptoms(phensFilename);
            LoadGenes(genesFilename);

            BuildSymptoms();
            BuildGenes();

            BuildMappings();
        }

        public string Prefix => "ORPHA";

        public Dictionary<string, Disease> Diseases { get; }
        public Dictionary<string, Symptom> Symptoms { get; }
        public Dictionary<string, Gene> Genes { get; }

        public Dictionary<string, HashSet<string>> Mappings { get; }
        public Dictionary<string, HashSet<string>> MappingsReverse { get; }

        private void LoadSymptoms(string filename)
        {
            var doc = XDocument.Load(filename);
            var nodes = doc.Descendants("HPODisorderSetStatus").Descendants("Disorder");
            foreach (var node in nodes)
            {
                var hpoAssocs = node.Element("HPODisorderAssociationList").Descendants("HPODisorderAssociation");

                var disease = GetOrCreateDisease(node);
                ParseSymptoms(disease, hpoAssocs);
            }
        }

        private void LoadGenes(string filename)
        {
            var doc = XDocument.Load(filename);
            var nodes = doc.Descendants("DisorderList").Descendants("Disorder");
            foreach (var node in nodes)
            {
                var geneAssocs = node.Element("DisorderGeneAssociationList").Descendants("DisorderGeneAssociation");

                // Add disease if not existing
                GetOrCreateDisease(node);

                var disease = GetParentDisease(node);
                if (disease != null)
                {
                    ParseGenes(disease, geneAssocs);
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

        private void BuildGenes()
        {
            foreach (var disease in Diseases.Values)
            {
                foreach (var gen in disease.Genes)
                {
                    var gene = GetOrCreateGene(gen.Id, gen.Name);
                    var assoc = new Association(disease.Id, disease.Name, gen.Type, gen.TypeDesc);
                    gene.Diseases.Add(assoc);
                }
            }
        }

        private void BuildMappings()
        {
            foreach (var disease in Diseases.Values)
            {
                Mappings.Add(disease.Id, disease.XRefs.Where(r => r.Value == "E").Select(r => r.Key).ToHashSet());
            }

            // Reverse mapping
            foreach (var item in Mappings)
            {
                foreach (var term in item.Value)
                {
                    MappingsReverse[term.ToUpper()] = new HashSet<string>();
                }
            }
            foreach (var item in Mappings)
            {
                foreach (var term in item.Value)
                {
                    MappingsReverse[term.ToUpper()].Add(item.Key);
                }
            }
        }

        private Disease GetOrCreateDisease(XElement node)
        {
            var sympDisease = ParseDisease(node);
            string id = BuildOrphaCode(node.Element("OrphaCode").Value);
            if (!Diseases.TryGetValue(id, out Disease disease))
            {
                disease = sympDisease;
                Diseases.Add(id, disease);
            }
            // Merge
            disease.Type = sympDisease.Type;
            disease.Group = sympDisease.Group;
            disease.Parent = sympDisease.Parent;
            return disease;
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

        private Gene GetOrCreateGene(string id, string name)
        {
            if (!Genes.TryGetValue(id, out Gene gene))
            {
                gene = new Gene(id, name);
                Genes.Add(id, gene);
            }
            return gene;
        }

        private Disease GetParentDisease(XElement node)
        {
            string id = BuildOrphaCode(node.Element("OrphaCode").Value);
            if (Diseases.TryGetValue(id, out Disease disease))
            {
                if (disease.Parent != null)
                {
                    return Diseases.TryGetValue(disease.Parent);
                }
                return disease;
            }
            return null;
        }

        static private readonly IList<string> EmptyMapping = new string[] { };

        public IList<string> OrphaToOmim(string orpha)
        {
            if (Mappings.ContainsKey(orpha))
            {
                return Mappings[orpha].Where(r => r.ToUpper().StartsWith("OMIM:")).ToArray();
            }
            return EmptyMapping;
        }

        public IList<string> OmimToOrpha(string omim)
        {
            if (MappingsReverse.ContainsKey(omim))
            {
                return MappingsReverse[omim].ToArray();
            }
            return EmptyMapping;
        }

        public bool Match(string local, string external)
        {
            if (Mappings.ContainsKey(local))
            {
                return Mappings[local].Contains(external);
            }
            return false;
        }

        static public string BuildOrphaCode(string id) => $"ORPHA:{id}";
    }
}
