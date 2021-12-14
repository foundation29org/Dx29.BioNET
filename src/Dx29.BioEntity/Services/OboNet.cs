using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    static public class OboNet
    {
        static public Dictionary<string, Term> Parse(string filename)
        {
            var terms = new Dictionary<string, Term>();
            ParseTerms(filename, terms);
            ResolveDependencies(terms);
            return terms;
        }

        static public Dictionary<string, Term> BuildAlternates(Dictionary<string, Term> terms)
        {
            var termsEx = new Dictionary<string, Term>();
            foreach (var term in terms.Values)
            {
                if (term.Alternates != null)
                {
                    foreach (var altId in term.Alternates)
                    {
                        termsEx.Add(altId, term);
                    }
                }
            }
            return termsEx;
        }

        static public void FillExternals(Dictionary<string, List<Term>> externals, Dictionary<string, Term> terms)
        {
            foreach (var termId in terms.Keys)
            {
                var term = terms[termId];
                if (term.XRefs != null)
                {
                    foreach (var xref in term.XRefs)
                    {
                        var xr = xref.Key.ToUpper();
                        if (!externals.ContainsKey(xr))
                        {
                            externals[xr] = new List<Term>();
                        }
                        externals[xr].Add(term);
                    }
                }
            }
        }

        static private void ParseTerms(string filename, Dictionary<string, Term> terms)
        {
            using (var reader = new StreamReader(filename))
            {
                var term = ReadTerm(reader);
                while (term != null)
                {
                    terms.Add(term.Id, term);
                    term = ReadTerm(reader);
                }
            }
        }

        static private void ResolveDependencies(Dictionary<string, Term> terms)
        {
            foreach (var term in terms.Values)
            {
                // Resolve children
                if (term.Parents != null)
                {
                    foreach (var reference in term.Parents)
                    {
                        var parent = terms[reference.Id];
                        reference.Name = terms[parent.Id].Name;
                        parent.Children ??= new List<Reference>();
                        parent.Children.Add(new Reference(term.Id) { Name = term.Name });
                    }
                }

                // Resolve obsolete
                if (term.IsObsolete)
                {
                    if (term.ReplacedBy != null)
                    {
                        string name = terms.ContainsKey(term.ReplacedBy.Id) ? terms[term.ReplacedBy.Id].Name : "";
                        if (name == "")
                        {
                            //Console.WriteLine("WARNING: Unresolved replaced_by in {0}:\t{1}", term.Id, term.ReplacedBy.Id);
                        }
                        term.ReplacedBy.Name = name;
                    }
                    if (term.Consider != null)
                    {
                        foreach (var consider in term.Consider)
                        {
                            string name = terms.ContainsKey(consider.Id) ? terms[consider.Id].Name : "";
                            if (name == "")
                            {
                                //Console.WriteLine("WARNING: Unresolved consider in {0}:\t{1}", term.Id, consider.Id);
                            }
                            consider.Name = name;
                        }
                    }
                }
            }
        }

        static private Term ReadTerm(StreamReader reader)
        {
            string line = reader.ReadLine();
            while (line != null)
            {
                if (line == "[Term]") break;
                line = reader.ReadLine();
            }
            if (line == "[Term]")
            {
                var term = new Term();
                line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Trim() == "") break;
                    ProcessLine(term, line);
                    line = reader.ReadLine();
                }
                return term;
            }
            return null;
        }

        static private void ProcessLine(Term term, string line)
        {
            int ix = line.IndexOf(':');
            string tag = line.Substring(0, ix);
            string val = line.Substring(ix + 1).Trim();
            switch (tag)
            {
                case "id":
                    term.Id = val;
                    break;
                case "name":
                    term.Name = val.UpperFirst();
                    break;
                case "def":
                    (var desc, var remain) = GetQuotedString(val);
                    term.Desc = desc;
                    term.PubMeds = ExtractPubMed(remain);
                    break;
                case "comment":
                    term.Comment = GetQuotedString(val).Item1.UpperFirst();
                    break;
                case "alt_id":
                    AddAlternate(term, val);
                    break;
                case "synonym":
                    AddSynonym(term, val);
                    break;
                case "is_a":
                    AddParent(term, val);
                    break;
                case "xref":
                    AddXRef(term, val);
                    break;
                case "is_obsolete":
                    term.IsObsolete = val == "true";
                    break;
                case "replaced_by":
                    term.ReplacedBy = new Reference(GetSafeId(val));
                    break;
                case "consider":
                    AddConsider(term, val);
                    break;
                default:
                    break;
            }
        }

        static private void AddAlternate(Term term, string val)
        {
            term.Alternates ??= new List<string>();
            term.Alternates.Add(GetSafeId(val));
        }

        static private void AddParent(Term term, string val)
        {
            term.Parents ??= new List<Reference>();
            term.Parents.Add(new Reference(GetSafeId(val)));
        }

        static private void AddXRef(Term term, string val)
        {
            var key = val.Split(' ')[0];
            var value = val.Substring(key.Length).Trim();
            if (value.Length > 0)
            {
                value = GetMondoRelation(value);
            }
            term.XRefs ??= new Dictionary<string, string>();
            term.XRefs.Add(key, value);
        }

        private static string GetMondoRelation(string val)
        {
            if (val.Contains("MONDO:equivalentTo")) return "Equivalent";
            else if (val.Contains("MONDO:relatedTo")) return "Related";
            else if (val.Contains("MONDO:superClassOf")) return "SuperClass";
            else if (val.Contains("MONDO:subClassOf")) return "SubClass";
            else if (val.Contains("MONDO:obsoleteEquivalent")) return "ObsoleteEquivalent";
            return "Unknown";
        }

        static private void AddConsider(Term term, string val)
        {
            term.Consider ??= new List<Reference>();
            term.Consider.Add(new Reference(GetSafeId(val)));
        }

        static private void AddSynonym(Term term, string val)
        {
            var synonym = new Synonym();
            (string label, string remain) = GetQuotedString(val);
            synonym.Label = label.UpperFirst();
            if (remain.Length > 0)
            {
                var parts = remain.Split(' ');
                string scope = parts[0];
                switch (scope)
                {
                    case "EXACT":
                    case "BROAD":
                    case "NARROW":
                    case "RELATED":
                        synonym.Scope = scope;
                        break;
                    default:
                        synonym.Scope = "RELATED";
                        break;
                }
                if (parts.Length > 1)
                {
                    if (!parts[1].StartsWith('['))
                    {
                        synonym.Type = parts[1];
                    }
                }
                int index = remain.IndexOf('[');
                if (index > 0)
                {
                    synonym.XRefs = remain.Substring(index);
                }
            }
            term.Synonyms ??= new List<Synonym>();
            term.Synonyms.Add(synonym);
        }

        static private (string, string) GetQuotedString(string value)
        {
            int index = 0;
            using (var writer = new StringWriter())
            {
                char last = Char.MinValue;
                for (; index < value.Length; index++)
                {
                    var c = value[index];
                    if (c == '\\')
                    {
                        last = c;
                        continue;
                    }
                    if (c == '\"')
                    {
                        if (index == 0)
                        {
                            last = c;
                            continue;
                        }
                        if (last == '\\')
                        {
                            writer.Write('\"');
                            last = c;
                            continue;
                        }
                        break;
                    }
                    writer.Write(c);
                    last = c;
                }
                string quote = writer.ToString().Trim();
                string remain = $"{value} ".Substring(index + 1).Trim();
                return (quote, remain);
            }
        }

        private static IList<string> ExtractPubMed(string val)
        {
            val = val.TrimStart('[').TrimEnd(']');
            if (!String.IsNullOrEmpty(val))
            {
                var parts = val.Split(',');
                var items = parts.Where(r => r.Trim().StartsWith("PMID:")).ToList();
                return items.Count > 0 ? items : null;
            }
            return null;
        }

        static private string UpperFirst(this string str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                if (Char.IsLower(str[0]))
                {
                    if (str.Length > 1)
                    {
                        return $"{Char.ToUpper(str[0])}{str.Substring(1)}";
                    }
                    return str.ToUpper();
                }
            }
            return str;
        }

        static private string GetSafeId(string val)
        {
            return val.Split(' ')[0].Replace('_', ':');
        }
    }
}
