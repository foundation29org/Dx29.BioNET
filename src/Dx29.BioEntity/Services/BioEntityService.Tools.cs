using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class BioEntityService
    {
        public IList<string> GetAncestors(IList<string> ids, int depth)
        {
            var items = new HashSet<string>();
            foreach (var id in ids)
            {
                foreach (var item in GetAncestors(id, depth))
                {
                    items.Add(item);
                }
            }
            return items.ToList();
        }

        public IList<string> GetAncestors(string id, int depth)
        {
            var ancentors = new HashSet<string>();

            var terms = GetHpoTerms(id);
            foreach (var term in terms)
            {
                foreach (var ancestor in GetAncestorsInternal(term.Id, depth))
                {
                    ancentors.Add(ancestor.Id);
                }
            }
            return ancentors.ToList();
        }

        private IEnumerable<Reference> GetAncestorsInternal(string id, int depth)
        {
            if (depth > 0)
            {
                var terms = GetHpoTerms(id);
                foreach (var term in terms)
                {
                    if (term.Parents != null)
                    {
                        foreach (var parent in term.Parents)
                        {
                            if (parent.Id != "HP:0000118")
                            {
                                foreach (var ancestor in GetAncestorsInternal(parent.Id, depth - 1))
                                {
                                    yield return ancestor;
                                }
                                yield return parent;
                            }
                        }
                    }
                }
            }
        }
    }
}
