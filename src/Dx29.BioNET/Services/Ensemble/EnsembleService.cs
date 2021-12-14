using System;
using System.Linq;
using System.Collections.Generic;

using Dx29.Data;

namespace Dx29.Services
{
    public partial class EnsembleService
    {
        public EnsembleService()
        {
            BioEntity = new BioEntityService();
            BioEntity.Initialize();

            Orpha = new OrphaNET();
            Orpha.Initialize();

            Omim = new Omim(BioEntity);
            Omim.Initialize();

            DiagnosisOrpha = new DiagnosisService(BioEntity, Orpha);
            DiagnosisOrpha.Initialize();

            DiagnosisOmim = new DiagnosisService(BioEntity, Omim);
            DiagnosisOmim.Initialize();

            OrphaTranslator = new OrphaTranslator();
            OrphaTranslator.Initialize("_data/ordo-es-3.2.owl");

            OmimTranslator = new BioTranslator();
            OmimTranslator.Initialize("_data/mondo-es.tsv");

            Omim.Mappings = Orpha.Mappings;
        }

        public BioEntityService BioEntity { get; }
        public OrphaNET Orpha { get; }
        public Omim Omim { get; }

        public DiagnosisService DiagnosisOrpha { get; }
        public DiagnosisService DiagnosisOmim { get; }

        public OrphaTranslator OrphaTranslator { get; }
        public BioTranslator OmimTranslator { get; }

        public DiagnosisResults Predict(string hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            return Predict(hpos.Split(',').Select(r => r.Trim()).ToArray(), genes, skip, count);
        }
        public DiagnosisResults Predict(IList<string> hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            var resOrpha = DiagnosisOrpha.Predict(hpos, genes, skip: 0, count: 5_000);
            var resOmim = DiagnosisOmim.Predict(hpos, genes, skip: 0, count: 5_000);

            //var results = Merge(resOrpha, resOmim);
            var results = MergeWithMapping(resOrpha, resOmim);
            var total = results.Count;
            var bestTotal = GetBestTotal(results);
            //results = results.Take(bestTotal).ToArray();

            var items = results.Skip(skip).Take(count).ToArray();

            return new DiagnosisResults
            {
                Diseases = items,
                Count = items.Count(),
                Total = total,
                BestTotal = bestTotal
            };
        }

        public DiagnosisResults Phrank(string hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            return Phrank(hpos.Split(',').Select(r => r.Trim()).ToArray(), genes, skip, count);
        }
        public DiagnosisResults Phrank(IList<string> hpos, IDictionary<string, double> genes = null, int skip = 0, int count = 100)
        {
            var resOrpha = DiagnosisOrpha.Phrank(hpos, genes, skip: 0, count: 5_000);
            var resOmim = DiagnosisOmim.Phrank(hpos, genes, skip: 0, count: 5_000);

            //var results = Merge(resOrpha, resOmim);
            var results = MergeWithMapping(resOrpha, resOmim);
            var total = results.Count;
            var bestTotal = GetBestTotal(results);
            //results = results.Take(bestTotal).ToArray();

            var items = results.Skip(skip).Take(count).ToArray();

            return new DiagnosisResults
            {
                Diseases = items,
                Count = items.Count(),
                Total = total,
                BestTotal = bestTotal
            };
        }

        private IList<DiseaseInfo> Merge(DiagnosisResults resOrpha, DiagnosisResults resOmim)
        {
            var dic = new Dictionary<string, DiseaseInfo>();

            int position = 0;
            foreach (var orpha in resOrpha.Diseases.OrderByDescending(r => r.Score))
            {
                var disease = new DiseaseInfo(orpha.Id, orpha.Name) { Desc = orpha.Desc };
                disease.Symptoms = orpha.Symptoms;
                disease.Genes = orpha.Genes;
                disease.Score = orpha.Score;
                disease.Position = position;
                dic.Add(orpha.Id, disease);
                position += 2;
            }

            position = 1;
            foreach (var omim in resOmim.Diseases.OrderByDescending(r => r.Score))
            {
                var disease = new DiseaseInfo(omim.Id, omim.Name) { Desc = omim.Desc };
                disease.Symptoms = omim.Symptoms;
                disease.Genes = omim.Genes;
                disease.Score = omim.Score;
                disease.Position = position;
                dic.Add(omim.Id, disease);
                position += 2;
            }

            return dic.Values.OrderBy(r => r.Position).OrderByDescending(r => r.Score).ToArray();
            //return dic.Values.OrderByDescending(r => r.Score).OrderBy(r => r.Position).ToArray();
            //return dic.Values.OrderByDescending(r => r.Score).ToArray();
            //return dic.Values.OrderBy(r => r.Position).ToArray();
        }

        private IList<DiseaseInfo> MergeWithMapping(DiagnosisResults resOrpha, DiagnosisResults resOmim)
        {
            var dic = new Dictionary<string, DiseaseInfo>();

            int position = 0;
            foreach (var orpha in resOrpha.Diseases.OrderByDescending(r => r.Score))
            {
                if (!dic.ContainsKey(orpha.Id))
                {
                    var disease = CreateDisease(orpha);
                    disease.Position = position;
                    dic.Add(orpha.Id, disease);
                }
                position += 2;
            }

            position = 1;
            foreach (var omim in resOmim.Diseases.OrderByDescending(r => r.Score))
            {
                var mappings = GetOmimMappings(omim).ToArray();
                if (mappings.Length > 0)
                {
                    foreach (var disease in mappings)
                    {
                        if (!dic.ContainsKey(disease.Id))
                        {
                            dic.Add(disease.Id, disease);
                        }
                    }
                }
                else
                {
                    var disease = CreateDisease(omim);
                    disease.Position = position;
                    dic.Add(omim.Id, disease);
                }
                position += 2;
            }

            return dic.Values.OrderBy(r => r.Position).OrderByDescending(r => r.Score).ToArray();
            //return dic.Values.OrderByDescending(r => r.Score).OrderBy(r => r.Position).ToArray();
            //return dic.Values.OrderByDescending(r => r.Score).ToArray();
            //return dic.Values.OrderBy(r => r.Position).ToArray();
        }

        private IEnumerable<DiseaseInfo> GetOmimMappings(DiseaseInfo omim)
        {
            foreach (var orphaId in Orpha.OmimToOrpha(omim.Id))
            {
                var orpha = Orpha.Diseases[orphaId];
                var disease = new DiseaseInfo(orpha.Id, orpha.Name) { Desc = orpha.Desc };
                disease.Symptoms = omim.Symptoms;
                disease.Genes = omim.Genes;
                disease.Score = omim.Score;
                yield return disease;
            }
        }

        private static DiseaseInfo CreateDisease(DiseaseInfo info)
        {
            var disease = new DiseaseInfo(info.Id, info.Name) { Desc = info.Desc };
            disease.Symptoms = info.Symptoms;
            disease.Genes = info.Genes;
            disease.Score = info.Score;
            return disease;
        }

        private int GetBestTotal(IList<DiseaseInfo> results)
        {
            var matches = results.Select(r => r.Symptoms.Count).ToHashSet();
            if (matches.Count < 2)
            {
                return results.Count;
            }
            int threshold = matches.OrderByDescending(r => r).Take(3).Last();
            return results.Where(r => r.Symptoms.Count >= threshold).Count();
        }
    }
}
