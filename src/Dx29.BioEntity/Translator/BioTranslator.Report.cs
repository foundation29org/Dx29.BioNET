using System;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class BioTranslator
    {
        public IEnumerable<(string, string, string, string)> GetMissing(IList<Term> terms, bool includeObsolete = false)
        {
            foreach (var term in terms)
            {
                if (!term.IsObsolete || includeObsolete)
                {
                    foreach (var missing in GetMissing(term))
                    {
                        yield return missing;
                    }
                }
            }
        }

        public IEnumerable<(string, string, string, string)> GetMissing(Term term)
        {
            string text = null;

            if (term.Name != null)
            {
                text = Translate(term.Name, true);
                if (text == null)
                {
                    yield return (term.Id, "NAME", HashString(term.Name), term.Name);
                }
            }

            if (term.Desc != null)
            {
                text = Translate(term.Desc, true);
                if (text == null)
                {
                    yield return (term.Id, "DEFTION", HashString(term.Desc), term.Desc);
                }
            }

            if (term.Comment != null)
            {
                text = Translate(term.Comment, true);
                if (text == null)
                {
                    yield return (term.Id, "COMMENT", HashString(term.Comment), term.Comment);
                }
            }

            if (term.Synonyms != null)
            {
                foreach (var item in term.Synonyms)
                {
                    if (item.Label != null)
                    {
                        text = Translate(item.Label, true);
                        if (text == null)
                        {
                            yield return (term.Id, "SYNONYM", HashString(item.Label), item.Label);
                        }
                    }
                }
            }
        }
    }
}
