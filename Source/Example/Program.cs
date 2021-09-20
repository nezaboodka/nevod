using System;
using System.IO;
using Nezaboodka.Nevod;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleExample();
            ComplexExample("Patterns/Basic/Basic.np", "LICENSE.txt");
        }

        static void SimpleExample()
        {
            var patternPackage = PatternPackage.FromText("#Google = {'Google', ~'Google Drive'};");
            var searchEngine = new TextSearchEngine(patternPackage);
            var searchResult = searchEngine.Search("Google but not Google Drive");
            foreach (var tag in searchResult.GetTags())
                Console.WriteLine(tag.GetText());
        }

        static void ComplexExample(string packageName, string textFile)
        {
            // Create package cache. It should be a global static object in real life
            var packageCache = new PackageCache();

            // Create pattern package
            var packageBuilder = new PackageBuilder(
                new PackageBuilderOptions() { SyntaxInformationBinding = true },
                packageCache);
            var patternPackage = packageBuilder.BuildPackageFromFile(packageName);

            // Create parsed text source from text file stream
            var textReader = new StreamReader(textFile);
            var textSource = new StreamTextSource(textReader, shouldCloseReader: true, 64_000);

            // Create search engine with specific candidate limits
            var searchEngine = new TextSearchEngine(
                patternPackage,
                new SearchOptions() { CandidateLimit = 100_000, PatternCandidateLimit = 10_000 });

            // Search and output pattern name for each match
            searchEngine.Search(textSource,
                (SearchEngine searchEngine, MatchedTag tag) =>
                {
                    Console.WriteLine(tag.PatternFullName);
                });
        }
    }
}
