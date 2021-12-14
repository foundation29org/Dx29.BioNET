using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class OrphaNET
    {
        private XNamespace owl = "http://www.w3.org/2002/07/owl#";
        private XNamespace obo = "http://purl.obolibrary.org/obo/";
        private XNamespace rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        private XNamespace efo = "http://www.ebi.ac.uk/efo/";
        private XNamespace skos = "http://www.w3.org/2004/02/skos/core#";
        private XNamespace rdfs = "http://www.w3.org/2000/01/rdf-schema#";

        private void LoadORDO()
        {
            LoadORDO(Path.Combine("_data", "ordo-en-3.2.owl"));
        }

        private void LoadORDO(string filename)
        {
            var doc = XDocument.Load(filename);
            var RDF = doc.Element(rdf + "RDF");

            foreach (var node in RDF.Elements(owl + "Class"))
            {
                var id = node.Element(skos + "notation")?.Value;
                if (id != null)
                {
                    var name = node.Element(rdfs + "label").Value;
                    var desc = node.Element(efo + "definition")?.Value;
                    var syms = GetSynonyms(node.Elements(efo + "alternative_term"));
                    var parent = GetClinicalType(node);
                    var xrefs = GetXRefs(node).ToDictionary(r => r.Item1, r => r.Item2);
                    var disease = new Disease(id, name) { Desc = desc, Parent = parent, XRefs = xrefs, Synonyms = syms.ToArray() };
                    Diseases.Add(id, disease);
                }
            }
        }

        private IEnumerable<string> GetSynonyms(IEnumerable<XElement> elems)
        {
            foreach (var elem in elems)
            {
                yield return elem.Value;
            }
        }

        private string GetClinicalType(XElement node)
        {
            var type = node.Elements(rdfs + "subClassOf").First().Attribute(rdf + "resource").Value;
            if (type.EndsWith("_377796"))
            {
                foreach (var restriction in node.Descendants(owl + "Restriction"))
                {
                    if (restriction.Element(owl + "onProperty").Attribute(rdf + "resource").Value.EndsWith("BFO_0000050"))
                    {
                        var orphanet = restriction.Element(owl + "someValuesFrom").Attribute(rdf + "resource").Value;
                        return orphanet.Split('/').Last().Replace("Orphanet_", "ORPHA:");
                    }
                }
            }
            return null;
        }

        private IEnumerable<(string, string)> GetXRefs(XElement node)
        {
            foreach (var item in node.NodesAfterSelf())
            {
                if (item.NodeType != System.Xml.XmlNodeType.Element) break;

                var elem = item as XElement;
                if (elem.Name.LocalName == "Axiom")
                {
                    var property = elem.Element(owl + "annotatedProperty").Attribute(rdf + "resource").Value;
                    if (property.EndsWith("hasDbXref"))
                    {
                        var code = elem.Element(owl + "annotatedTarget").Value;
                        var type = elem.Element(obo + "ECO_0000218").Value.Split(' ').First();
                        yield return (code, type);
                    }
                }
            }
        }
    }
}
