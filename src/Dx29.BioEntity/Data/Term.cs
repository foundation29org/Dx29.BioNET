using System;
using System.Collections.Generic;

namespace Dx29.Data
{
    public class Term
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Comment { get; set; }

        public IList<string> Alternates { get; set; }
        public IList<Synonym> Synonyms { get; set; }
        public IList<Reference> Categories { get; set; }
        public IList<Reference> Parents { get; set; }
        public IList<Reference> Children { get; set; }
        public IList<string> PubMeds { get; set; }
        public IDictionary<string, string> XRefs { get; set; }

        public bool IsObsolete { get; set; }
        public Reference ReplacedBy { get; set; }
        public IList<Reference> Consider { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }

    public class Synonym
    {
        public Synonym()
        {
            Scope = "RELATED";
        }

        public string Label { get; set; }
        public string Scope { get; set; }
        public string Type { get; set; }
        public string XRefs { get; set; }
    }

    public class Reference
    {
        public Reference() { }
        public Reference(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
        public string Name { get; set; }

        static public Reference FromTerm(Term term)
        {
            return new Reference(term.Id) { Name = term.Name };
        }
    }
}
