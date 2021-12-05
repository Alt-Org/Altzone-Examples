using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity.Localization
{
    [Serializable]
    public class Language
    {
        [SerializeField] private SystemLanguage _language;
        [SerializeField] private string _localeName;

        private Dictionary<string, string> _words;

        public SystemLanguage LanguageName => _language;
        public string Locale => _localeName;

        public string Word(string key) => _words.TryGetValue(key, out var value) ? value : $"[{key}]";

        public Language(SystemLanguage language, string localeName, Dictionary<string, string> words)
        {
            _language = language;
            _localeName = localeName;
            _words = words;
        }
    }

    [Serializable]
    public class Languages
    {
        [SerializeField] private List<Language> _languages;

        public void Clear()
        {
            _languages.Clear();
        }

        public void Add(Language language)
        {
            var index = _languages.FindIndex(x => x.Locale == language.Locale);
            if (index != -1)
            {
                _languages.RemoveAt(index);
            }
            _languages.Add(language);
        }
    }

    public static class Localizer
    {
        private static Languages _languages;
        private const string DefaultLocale = "en";

        // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
        private static readonly string[] SupportedLocales = {
            "key",
            "en",
            "fi",
            "sv"
        };

        private static readonly SystemLanguage[] SupportedLanguages = {
            SystemLanguage.Unknown,
            SystemLanguage.English,
            SystemLanguage.Finnish,
            SystemLanguage.Swedish
        };

        public static SystemLanguage DefaultLanguage => SystemLanguage.Finnish;

        public static void SetLanguage(SystemLanguage language)
        {
            Debug.Log($"SetLanguage {language}");
        }

        public static void LoadTranslations()
        {
            var config = Resources.Load<LocalizationConfig>(nameof(LocalizationConfig));
            Assert.IsNotNull(config, "config != null");
            var text = config.TranslationsFile;
            Debug.Log($"TranslationsFileName {config.TranslationsFile.name} data len {text.text.Length}");
            if (_languages == null)
            {
                _languages = new Languages();
            }
            else
            {
                _languages.Clear();
            }
            LoadTranslations(text.text);
        }

        private static void LoadTranslations(string lines)
        {
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
            for (var i = 1; i < maxIndex; ++i)
            {
                var lang = SupportedLanguages[i];
                var locale = SupportedLocales[i];
                var dictionary = dictionaries[i];
                var language = new Language(lang, locale, dictionary);
                _languages.Add(language);
                if (i == 1)
                {
                    Debug.Log($"wordCount {dictionary.Count} in  {locale} {lang}");
                }
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