using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity.Localization
{
    public class Language
    {
        private readonly SystemLanguage _language;
        private readonly string _localeName;

        private readonly Dictionary<string, string> _words;
        private readonly Dictionary<string, string> _altWords;

        public SystemLanguage LanguageName => _language;
        public string Locale => _localeName;

        public string Word(string key) =>
            _words.TryGetValue(key, out var value) ? value
            : _altWords.TryGetValue(key, out var altValue) ? altValue
            : $"[{key}]";

        public Language(SystemLanguage language, string localeName, Dictionary<string, string> words, Dictionary<string, string> altWords)
        {
            _language = language;
            _localeName = localeName;
            _words = words;
            _altWords = altWords;
        }
    }

    public class Languages
    {
        private readonly List<Language> _languages = new List<Language>();

        public void Add(Language language)
        {
            Assert.IsTrue(_languages.FindIndex(x => x.Locale == language.Locale) == -1,
                "_languages.FindIndex(x => x.Locale == language.Locale) == -1");
            _languages.Add(language);
        }
    }

    public static class Localizer
    {
        private static Languages _languages;
        private const string DefaultLocale = "en";

        // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
        private static readonly string[] SupportedLocales =
        {
            "key",
            "en",
            "fi",
            "sv"
        };

        private static readonly SystemLanguage[] SupportedLanguages =
        {
            SystemLanguage.Unknown,
            SystemLanguage.English,
            SystemLanguage.Finnish,
            SystemLanguage.Swedish
        };

        public const SystemLanguage DefaultLanguage = SystemLanguage.Finnish;

        public static void SetLanguage(SystemLanguage language)
        {
            Debug.Log($"SetLanguage {language}");
        }

        public static void LoadTranslations()
        {
            var config = Resources.Load<LocalizationConfig>(nameof(LocalizationConfig));
            Assert.IsNotNull(config, "config != null");
            var textAsset = config.TranslationsTsvFile;
            Debug.Log($"Translations tsv {textAsset.name} text len {textAsset.text.Length}");
            _languages = new Languages();
            LoadTranslations(textAsset.text);
            var binAsset = config.LanguagesBinFile;
            Debug.Log($"Languages bin {binAsset.name} bytes len {binAsset.bytes.Length}");
        }

        private static void LoadTranslations(string lines)
        {
            var languages = new Languages();
            var maxIndex = SupportedLocales.Length;
            var dictionaries = new Dictionary<string, string>[maxIndex];
            var lineCount = 0;
            using (var reader = new StringReader(lines))
            {
                var line = reader.ReadLine();
                Assert.IsNotNull(line, "line != null");
                lineCount += 1;
                Debug.Log($"FIRST LINE: {line.Replace('\t', ' ')}");
                // key en fi sv es ru it de fr zh-CN
                var tokens = line.Split('\t');
                Assert.IsTrue(tokens.Length >= maxIndex, "tokens.Length >= maxIndex");
                for (var i = 1; i < maxIndex; ++i)
                {
                    // Translation file columns must match our understanding of locale columns!
                    var requiredLocale = SupportedLocales[i];
                    var currentLocale = tokens[i];
                    Assert.IsTrue(currentLocale == requiredLocale, "currentLocale == requiredLocale");
                    if (i == 1)
                    {
                        // Default locale must be in first locale column.
                        Assert.IsTrue(currentLocale == DefaultLocale, "currentLocale == _defaultLocale");
                    }
                    dictionaries[i] = new Dictionary<string, string>();
                }
                for (;;)
                {
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    lineCount += 1;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    ParseLine(line, maxIndex, ref dictionaries);
                }
            }
            Debug.Log($"lineCount {lineCount}");
            Dictionary<string, string> altDictionary = null;
            for (var i = 1; i < maxIndex; ++i)
            {
                var lang = SupportedLanguages[i];
                var locale = SupportedLocales[i];
                var dictionary = dictionaries[i];
                if (i == 1)
                {
                    Debug.Log($"wordCount {dictionary.Count} in  {locale} {lang}");
                    altDictionary = dictionary;
                }
                var language = new Language(lang, locale, dictionary, altDictionary);
                _languages.Add(language);
            }
        }

        private static void ParseLine(string line, int maxIndex, ref Dictionary<string, string>[] dictionaries)
        {
            var tokens = line.Split('\t');
            Assert.IsTrue(tokens.Length >= maxIndex, "tokens.Length >= maxIndex");
            var key = tokens[0];
            var defaultValue = tokens[1];
            Assert.IsFalse(string.IsNullOrEmpty(defaultValue), "string.IsNullOrEmpty(defaultValue)");
            for (var i = 1; i < maxIndex; ++i)
            {
                var colValue = string.IsNullOrEmpty(tokens[i]) ? defaultValue : tokens[i];
                var dictionary = dictionaries[i];
                dictionary.Add(key, colValue);
            }
        }
    }
}