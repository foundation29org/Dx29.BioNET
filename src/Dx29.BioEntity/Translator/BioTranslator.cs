using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Dx29.Services
{
    public partial class BioTranslator
    {
        public BioTranslator()
        {
            MD5Hash = MD5.Create();
        }

        public MD5 MD5Hash { get; }
        public Dictionary<string, string> Dictionary { get; set; }

        public void Initialize(string filename)
        {
            LoadTranslationTerms(filename);
        }

        private void LoadTranslationTerms(string filename)
        {
            Dictionary = new Dictionary<string, string>();

            using (var reader = new StreamReader(filename))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    var parts = line.Split('\t');
                    Dictionary[parts[2]] = parts[4];
                    line = reader.ReadLine();
                }
            }
        }

        public string Translate(string text, bool nullIfNoExists = false)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                var hash = HashString(text);
                if (Dictionary.TryGetValue(hash, out string trans))
                {
                    return trans;
                }
                return nullIfNoExists ? null : text;
            }
            return text;
        }

        private string HashString(string str)
        {
            var hash = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
