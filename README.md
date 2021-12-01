# Nevod

[![Nuget](https://img.shields.io/nuget/v/Nezaboodka.Nevod)](https://www.nuget.org/packages/Nezaboodka.Nevod/)

This repo contains the sources of the Nevod library, as well as the code to build the Nevod NuGet package
and the Negrep command-line utility for all supported platforms.

## What is Nevod?

Nevod is a **language** and technology for **pattern-based** text search. It is specially
aimed to rapidly reveal entities and their relationships in texts written in the natural language.

- Official Website: https://nevod.io/
- Language Tutorial: https://nevod.io/#/tutorial
- Language Reference: https://nevod.io/#/reference
- Playground: https://nevod.io/#/playground

![Nevod Playground](https://raw.githubusercontent.com/nezaboodka/nevod/main/nevod.jpg)

The **Nevod Web site** is also an open-source project, hosted on GitLab: https://gitlab.com/nezaboodka/nevod.website

## The Target Platform

The Nevod technology is based on the .NET Standard 2.1. It runs on Linux, Windows and macOS.

## How Can I Use Nevod in My Project?

You can use Nevod in the following ways:
- Download [source code](https://github.com/nezaboodka/nevod) and link it into your source tree.
- Add reference to the Nevod [NuGet package](https://www.nuget.org/packages/Nezaboodka.Nevod).
- Download and use the command-line [negrep](https://nevod.io/#/downloads) utility, which is pre-compiled for
Windows, Linux and macOS.

## Example of Usage in C# Code

Below is the simple example that demonstrates usage of the Nevod library in C# code.

```csharp
static void SimpleExample()
{
    var patternPackage = PatternPackage.FromText("#Google = {'Google', ~'Google Drive'};");
    var searchEngine = new TextSearchEngine(patternPackage);
    var searchResult = searchEngine.Search("Google but not Google Drive");
    foreach (var tag in searchResult.GetTags())
        Console.WriteLine(tag.GetText());
}
```

In the above example the pattern package is created from text with pattern definition. Then the
search engine is created and used to search for the pattern in a given text string.

You may find this example in the [Source/Example/Program.cs](Source/Example/Program.cs) file.

There is also advanced example, which demonstrates advanced techniques like usage of
package cache, package builder with options, stream-based text source, search options,
as well as search result handling callback.

```csharp
static void AdvancedExample(string packageName, string textFile)
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
```

## License

Nevod is licensed under [Apache 2.0](LICENSE.txt) license, which includes the patent license.
