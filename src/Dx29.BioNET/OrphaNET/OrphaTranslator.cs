using System;
using System.Collections.Generic;
using System.Xml.Linq;

using Dx29.Data;

namespace Dx29.Services
{
    public class OrphaTranslator
    {
        private XNamespace owl = "http://www.w3.org/2002/07/owl#";
        private XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private XNamespace efo = "http://www.ebi.ac.uk/efo/";
        private XNamespace skos = "http://www.w3.org/2004/02/skos/core#";
        private XNamespace rdfs = "http://www.w3.org/2000/01/rdf-schema#";

        public Dictionary<string, DiseaseDesc> DiseaseDescs { get; private set; }

        public void Initialize(string filename)
        {
            DiseaseDescs = new Dictionary<string, DiseaseDesc>();

            var doc = XDocument.Load(filename);
            var RDF = doc.Element(rdf + "RDF");

            foreach (var node in RDF.Elements(owl + "Class"))
            {
                var id = node.Element(skos + "notation")?.Value;
                if (id != null)
                {
                    var name = node.Element(rdfs + "label").Value;
                    var desc = node.Element(efo + "definition")?.Value;
                    var diseaseDesc = new DiseaseDesc
                    {
                        Id = id,
                        Name = name,
                        Desc = desc
                    };
                    DiseaseDescs[id] = diseaseDesc;
                }
            }
        }

        public DiseaseDesc Translate(DiseaseDesc diseaseDesc)
        {
            if (DiseaseDescs.ContainsKey(diseaseDesc.Id))
            {
                var trans = DiseaseDescs[diseaseDesc.Id];
                diseaseDesc.Name = trans.Name;
                diseaseDesc.Desc = trans.Desc;
            }
            return diseaseDesc;
        }
    }
}
