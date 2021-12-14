using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Dx29.Data;

namespace Dx29.Services
{
    partial class OrphaNET
    {
        private Disease ParseDisease(XElement node)
        {
            var id = BuildOrphaCode(node.Element("OrphaCode").Value);
            var name = node.Element("Name").Value;
            var item = new Disease(id, name)
            {
                //Link = node.Element("ExpertLink").Value,
                Type = node.Element("DisorderType").Attribute("id").Value.AsInt32(),
                Group = node.Element("DisorderGroup").Attribute("id").Value.AsInt32()
            };
            return item;
        }

        private void ParseGenes(Disease disease, IEnumerable<XElement> associations)
        {
            foreach (var assoc in associations)
            {
                var gene = assoc.Element("Gene");
                var geneId = gene.Element("Symbol").Value;
                var geneName = GetGeneHGNC(gene);

                var assocType = assoc.Element("DisorderGeneAssociationType");
                var assocTypeId = assocType.Attribute("id").Value;
                var assocTypeName = assocType.Element("Name").Value;

                disease.Genes.Add(new Association(geneId, geneName, assocTypeId, assocTypeName));
            }
        }

        private void ParseSymptoms(Disease disease, IEnumerable<XElement> associations)
        {
            foreach (var assoc in associations)
            {
                var hpo = assoc.Element("HPO");
                var hpoId = hpo.Element("HPOId").Value;
                var hpoName = hpo.Element("HPOTerm").Value;

                var assocType = assoc.Element("HPOFrequency");
                var assocTypeId = ConvertFrequency(assocType.Attribute("id").Value);
                var assocTypeName = assocType.Element("Name").Value;

                disease.Symptoms.Add(new Association(hpoId, hpoName, assocTypeId, assocTypeName));
            }
        }

        private string GetGeneHGNC(XElement element)
        {
            foreach (var item in element.Element("ExternalReferenceList").Elements("ExternalReference"))
            {
                if (item.Element("Source").Value == "HGNC")
                {
                    return $"HGNC:{item.Element("Reference").Value}";
                }
            }
            return element.Element("Symbol").Value;
        }

        private string ConvertFrequency(string type)
        {
            switch (type.ToUpper())
            {
                case "28405":
                    return "HP:0040280";
                case "28412":
                    return "HP:0040281";
                case "28419":
                    return "HP:0040282";
                case "28426":
                    return "HP:0040283";
                case "28433":
                    return "HP:0040284";
                case "28440":
                    return "HP:0040285";
            }
            return "HP:9999999";
        }
    }
}
