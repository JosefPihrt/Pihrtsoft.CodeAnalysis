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

            _ = WordListLoader.LoadFile(_wordListDirPath + @"\ignore.txt");
            WordList abbreviations = WordListLoader.LoadFile(_wordListDirPath + @"\abbreviations.txt").List;
            WordList acronyms = WordListLoader.LoadFile(_wordListDirPath + @"\acronyms.txt").List;
            WordList br = WordListLoader.LoadFile(_wordListDirPath + @"\br.txt").List;
            WordList us = WordListLoader.LoadFile(_wordListDirPath + @"\us.txt").List;
            WordList fonts = WordListLoader.LoadFile(_wordListDirPath + @"\it\fonts.txt").List;
            WordList languages = WordListLoader.LoadFile(_wordListDirPath + @"\it\languages.txt").List;
            WordList names = WordListLoader.LoadFile(_wordListDirPath + @"\names.txt").List;
            WordList plural = WordListLoader.LoadFile(_wordListDirPath + @"\plural.txt").List;
            WordList science = WordListLoader.LoadFile(_wordListDirPath + @"\science.txt").List;

            WordList geography = WordListLoader.Load(
                Directory.EnumerateFiles(
                    _wordListDirPath + @"\geography",
                    "*.*",
                    SearchOption.AllDirectories)).List;

            WordList it = WordListLoader.Load(
                Directory.EnumerateFiles(
                    _wordListDirPath + @"\it",
                    "*.*",
                    SearchOption.AllDirectories)).List;

            WordList math = WordListLoader.LoadFile(_wordListDirPath + @"\math.txt")
                .List
                .Except(abbreviations, acronyms, fonts);

            WordList @default = WordListLoader.LoadFile(_wordListDirPath + @"\default.txt")
                .List
                .Except(br, us, geography);

            WordList custom = WordListLoader.LoadFile(_wordListDirPath + @"\custom.txt")
                .List
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
                WordListLoader.LoadFile(_wordListDirPath + @"\it\main.txt").List,
                WordListLoader.LoadFile(_wordListDirPath + @"\it\names.txt").List);

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
