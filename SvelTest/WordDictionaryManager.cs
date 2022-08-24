using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SvelTest
{
    public static class WordDictionaryManager
    {
        private static ApplicationContext context = new ApplicationContext(new DbContextOptions<ApplicationContext>());

        public static void CreateDictionary(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile)) return;
            ClearDictionary();

            string readedText = "";
            using (var reader = new StreamReader(pathToFile, Encoding.UTF8))
            {
                readedText = reader.ReadToEnd();
            }
            readedText = Regex.Replace(readedText, @"\W", " ");
            readedText = Regex.Replace(readedText, "[0-9]", " ", RegexOptions.IgnoreCase);

            string[] words = readedText.Trim().Split(' ', '\r', '\n'); // ',', '.', '!', '?', ':', '-', '>',

            Dictionary<string, int> freqWords = new Dictionary<string, int>();
            foreach (string word in words)
            {
                if (word.Length < 3) continue;

                int wordCount = words.Count(x => x == word);
                if (wordCount >= 3 && !freqWords.ContainsKey(word))
                {
                    freqWords.Add(word, wordCount);
                }
            }

            foreach (var word in freqWords)
            {
                context.FrequentlyUsedWords.Add(new FrequentlyUsedWord() { Text = word.Key, Count = word.Value });
            }
            context.SaveChanges();
        }

        public static void UpdateDictionary(string pathToFile)
        {
            if (string.IsNullOrEmpty(pathToFile)) return;

            string readedText = "";
            using (var reader = new StreamReader(pathToFile, Encoding.UTF8))
            {
                readedText = reader.ReadToEnd();
            }
            string[] words = readedText.Trim().Split(' ', ',', '.', '!', '?', ':', '-', '>', '\r', '\n'); // и т.д

            Dictionary<string, int> freqWords = new Dictionary<string, int>();
            foreach (string word in words)
            {
                if (word.Length < 3) continue;
                int wordCount = words.Count(x => x == word);

                var sameWord = context.FrequentlyUsedWords.FirstOrDefault(x => x.Text == word);
                if (sameWord != null)
                {
                    sameWord.Count++;
                }
                else
                {
                    if (wordCount >= 3 && !freqWords.ContainsKey(word))
                    {
                        freqWords.Add(word, wordCount);
                    }
                }

            }

            foreach (var word in freqWords)
            {
                context.FrequentlyUsedWords.Add(new FrequentlyUsedWord() { Text = word.Key, Count = word.Value });
            }
            context.SaveChanges();
        }

        public static void ClearDictionary()
        {
            context.FrequentlyUsedWords.RemoveRange(context.FrequentlyUsedWords);
            context.SaveChanges();
            context.Database.ExecuteSqlRaw("TRUNCATE TABLE [FrequentlyUsedWords]");
        }

        public static string[] GetSuggestedWords(string text)
        {
            if (string.IsNullOrEmpty(text)) return new string[0];
            return context.FrequentlyUsedWords.Where(x => x.Text.StartsWith(text)).OrderBy(x => x.Text).Take(5).OrderByDescending(x => x.Count).Select(x => x.Text).ToArray();
        }

    }
}
