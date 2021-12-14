using System;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    public interface IOntology
    {
        string Prefix { get; }

        Dictionary<string, Disease> Diseases { get; }
        Dictionary<string, Symptom> Symptoms { get; }
        Dictionary<string, Gene> Genes { get; }

        bool Match(string local, string external);
    }
}
