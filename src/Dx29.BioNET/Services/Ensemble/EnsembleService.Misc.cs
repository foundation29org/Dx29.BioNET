using System;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    partial class EnsembleService
    {
        public void TranslateDiseases(IList<DiffDisease> diseases, string lang = "es")
        {
            foreach (var disease in diseases)
            {
                var trans = DescribeDisease(disease.Id, lang);
                disease.Name = trans.Name;
                disease.Desc = trans.Desc;
            }
        }

        public IDictionary<string, DiseaseDesc> DescribeDiseases(IList<string> ids, string lang = "en")
        {
            var results = new Dictionary<string, DiseaseDesc>();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var diseaseDesc = DescribeDisease(id, lang);
                    results[id] = diseaseDesc;
                }
            }
            return results;
        }

        public DiseaseDesc DescribeDisease(string id, string lang)
        {
            if (id.ToUpper().StartsWith("ORPHA:"))
            {
                return DescribeDiseaseOrpha(id, lang);
            }
            return DescribeDiseaseOmim(id, lang);
        }

        public DiseaseDesc DescribeDiseaseOrpha(string id, string lang)
        {
            var diseaseDesc = new DiseaseDesc(id, id);
            var disease = Orpha.Diseases.TryGetValue(id);
            if (disease != null)
            {
                diseaseDesc.Name = disease.Name;
                diseaseDesc.Desc = disease.Desc;
                if (lang != "en")
                {
                    OrphaTranslator.Translate(diseaseDesc);
                }
            }
            return diseaseDesc;
        }

        public DiseaseDesc DescribeDiseaseOmim(string id, string lang)
        {
            var diseaseDesc = new DiseaseDesc(id, id);
            var disease = Omim.Diseases.TryGetValue(id);
            if (disease != null)
            {
                diseaseDesc.Name = disease.Name;
                diseaseDesc.Desc = disease.Desc;
                if (lang != "en")
                {
                    diseaseDesc.Name = OmimTranslator.Translate(diseaseDesc.Name);
                    diseaseDesc.Desc = OmimTranslator.Translate(diseaseDesc.Desc);
                }
            }
            return diseaseDesc;
        }
    }
}
