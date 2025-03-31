using DrugIndication.Domain.Models;
using System.Text.RegularExpressions;

namespace DrugIndication.Parsing.Services
{
    public class Icd10Service
    {
        private readonly List<Icd10Code> _codes;

        public Icd10Service(string csvPath)
        {
            _codes = LoadFromCsv(csvPath);
        }

        // Load ICD-10 codes from a CSV file
        private List<Icd10Code> LoadFromCsv(string path)
        {
            var codes = new List<Icd10Code>();
            var lines = File.ReadAllLines(path).Skip(1); // skip header

            foreach (var line in lines)
            {
                var parts = line.Split(',');

                if (parts.Length >= 2)
                {
                    codes.Add(new Icd10Code
                    {
                        Code = parts[0].Trim(),
                        Description = parts[1].Trim()
                    });
                }
            }

            return codes;
        }

        // Search the best match using string similarity (basic version)
        public Icd10Code? FindClosestMatch(string indication)
        {
            if (string.IsNullOrWhiteSpace(indication)) return null;

            indication = indication.ToLower();

            // Try exact or partial match first
            var partial = _codes.FirstOrDefault(c =>
                indication.Contains(c.Description.ToLower()) ||
                c.Description.ToLower().Contains(indication));

            if (partial != null)
                return partial;

            // Try word-based fuzzy match (very basic)
            var words = Regex.Split(indication, @"\W+").Where(w => w.Length > 3).ToList();

            foreach (var word in words)
            {
                var match = _codes.FirstOrDefault(c => c.Description.ToLower().Contains(word));
                if (match != null)
                    return match;
            }

            return null;
        }
    }
}
