// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Roslynator.Spelling;

namespace Roslynator.CommandLine
{
    internal static class WordListHelpers
    {
        private static readonly Regex _splitRegex = new Regex(" +");

        private const string _wordListDirPath = @"..\..\..\Spelling\words";
        private const string _fixListDirPath = @"..\..\..\Spelling\fixes";

        public static void ProcessWordLists()
        {
            foreach (string filePath in Directory.EnumerateFiles(_wordListDirPath, "*.txt", SearchOption.AllDirectories))
            {
                WordList.Normalize(filePath);
            }

            _ = WordList.LoadFile(_wordListDirPath + @"\ignore.txt");
            WordList abbreviations = WordList.LoadFile(_wordListDirPath + @"\abbreviations.txt");
            WordList acronyms = WordList.LoadFile(_wordListDirPath + @"\acronyms.txt");
            WordList br = WordList.LoadFile(_wordListDirPath + @"\br.txt");
            WordList us = WordList.LoadFile(_wordListDirPath + @"\us.txt");
            WordList fonts = WordList.LoadFile(_wordListDirPath + @"\it\fonts.txt");
            WordList languages = WordList.LoadFile(_wordListDirPath + @"\it\languages.txt");
            WordList names = WordList.LoadFile(_wordListDirPath + @"\names.txt");
            WordList plural = WordList.LoadFile(_wordListDirPath + @"\plural.txt");
            WordList science = WordList.LoadFile(_wordListDirPath + @"\science.txt");

            WordList geography = WordList.LoadFiles(
                Directory.EnumerateFiles(
                    _wordListDirPath + @"\geography",
                    "*.*",
                    SearchOption.AllDirectories));

            WordList it = WordList.LoadFiles(
                Directory.EnumerateFiles(
                    _wordListDirPath + @"\it",
                    "*.*",
                    SearchOption.AllDirectories));

            WordList math = WordList.LoadFile(_wordListDirPath + @"\math.txt")
                .Except(abbreviations, acronyms, fonts);

            WordList @default = WordList.LoadFile(_wordListDirPath + @"\default.txt")
                .Except(br, us, geography);

            WordList custom = WordList.LoadFile(_wordListDirPath + @"\custom.txt")
                .Except(
                    abbreviations,
                    acronyms,
                    br,
                    us,
                    geography,
                    math,
                    names,
                    plural,
                    science,
                    it);

            WordList all = @default.AddValues(
                custom,
                br,
                us,
                languages,
                math,
                plural,
                abbreviations,
                acronyms,
                names,
                geography,
                WordList.LoadFile(_wordListDirPath + @"\it\main.txt"),
                WordList.LoadFile(_wordListDirPath + @"\it\names.txt"));

            ProcessFixList(all);
        }

        private static void ProcessFixList(WordList wordList)
        {
            const string path = _fixListDirPath + @"\fixes.txt";

            FixList fixList = FixList.LoadFile(path);

            foreach (KeyValuePair<string, ImmutableHashSet<SpellingFix>> kvp in fixList.Items)
            {
                if (wordList.Contains(kvp.Key))
                    Debug.Fail(kvp.Key);

                foreach (SpellingFix fix in kvp.Value)
                {
                    string value = fix.Value;

                    foreach (string value2 in _splitRegex.Split(value))
                    {
                        if (!wordList.Contains(value2))
                            Debug.Fail($"{value}: {value2}");
                    }
                }
            }

            fixList.SaveAndLoad(path);
        }
    }
}
