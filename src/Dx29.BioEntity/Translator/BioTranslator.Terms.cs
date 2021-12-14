using System;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class BioTranslator
    {
        public void Translate(IList<Term> terms)
        {
            foreach (var term in terms)
            {
                Translate(term);
            }
        }

        public void Translate(Term term)
        {
            term.Name = Translate(term.Name);
            term.Desc = Translate(term.Desc);
            term.Comment = Translate(term.Comment);

            if (term.Synonyms != null)
            {
                foreach (var item in term.Synonyms)
                {
                    item.Label = Translate(item.Label);
                }
            }
            if (term.Categories != null)
            {
                foreach (var item in term.Categories)
                {
                    item.Name = Translate(item.Name);
                }
            }
            if (term.Parents != null)
            {
                foreach (var item in term.Parents)
                {
                    item.Name = Translate(item.Name);
                }
            }
            if (term.Children != null)
            {
                foreach (var item in term.Children)
                {
                    item.Name = Translate(item.Name);
                }
            }
        }
    }
}
