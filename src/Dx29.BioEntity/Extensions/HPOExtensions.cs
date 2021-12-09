using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    static public class HPOExtensions
    {
        static public Dictionary<string, Term> AssignCategories(this Dictionary<string, Term> terms)
        {
            foreach (var cat in terms["HP:0000118"].Children)
            {
                foreach (var termRef in terms[cat.Id].Children)
                {
                    var term = terms[termRef.Id];
                    AssignCategories(terms, cat, term);
                }
            }
            return terms;
        }

        private static void AssignCategories(Dictionary<string, Term> terms, Reference category, Term term)
        {
            term.Categories ??= new List<Reference>();
            if (!term.Categories.Any(r => r.Id == category.Id))
            {
                term.Categories.Add(category);
            }
            if (term.Children != null)
            {
                foreach (var child in term.Children)
                {
                    AssignCategories(terms, category, terms[child.Id]);
                }
            }
        }
    }
}
