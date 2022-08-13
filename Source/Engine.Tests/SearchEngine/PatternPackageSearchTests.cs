//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("PatternPackageSearch")]
    public class PatternPackageSearchTests
    {
        [TestMethod]
        public void NamespaceWithPatternsHavingQualifiedReferences()
        {
            string package = @"
@namespace NZ
{
    @pattern #SiteOrBlog = {NZ.Site, NZ.Blog};
    @pattern Blog = ?'blog.' + ?(Word + '.') + 'com';
    @pattern Site = ?'nezaboodka.' + 'com';
}
";
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(GetFileContent, package, text,
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void NamespaceWithPatternsHavingShortReferences()
        {
            string package = @"
@namespace NZ
{
    @pattern #Nezaboodka = 'nezaboodka' + Space + TitleWord;
    @pattern #SiteOrBlog = {Site, Blog};
    @pattern Site = ?'nezaboodka.' + 'com';
    @pattern Blog = ?'blog.' + ?(Word + '.') + 'com';
}

@pattern TitleWord = Alpha(3-20, TitleCase);
@pattern #TitleWordPrefix = 'Ne'!*(Alpha, 7-10, Lowercase);
";
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(GetFileContent, package, text,
                "Nezaboodka",
                "Nezaboodka Software",
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void NamespaceWithNestedPatternsHavingShortReferences()
        {
            string package = @"
@namespace NZ
{
    @pattern #Nezaboodka = 'nezaboodka' + Space + TitleWord
    @where
    {
        @pattern #SiteOrBlog = {Site, Blog}
        @where
        {
            Site = ?'nezaboodka.' + 'com';
            Blog = ?'blog.' + ?(Word + '.') + 'com';
        };
    };
}

@pattern TitleWord = Alpha(3-20, TitleCase);
@pattern #TitleWordPrefix = 'Ne'!*(Alpha, 7-10, Lowercase);
";
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(GetFileContent, package, text,
                "Nezaboodka",
                "Nezaboodka Software",
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void SearchPackage()
        {
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(GetFileContent, UrlFileContent, text,
                "www.google.com");
        }

        [TestMethod]
        public void SearchPatternsInRequiredPackages()
        {
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(GetFileContent, SearchDomainFileContent, text,
                "www.google.com");
        }

        [TestMethod]
        public void SearchTargetPatternsInRequiredPackages()
        {
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: http://blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(GetFileContent, SearchUrlAndDomainFileContent, text,
                "blog.nezaboodka.com",
                "http://blog.nezaboodka.com");
        }

        [TestMethod]
        public void RequiredPackageReferencedInOtherPackages()
        {
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(GetFileContent, SearchContactsFileContent, text,
                "www.google.com");
        }

        [TestMethod]
        public void SearchUrlFieldsPackage()
        {
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: " +
                "https://blog.nezaboodka.com/post/2019/599-nevod-technology-for-pattern-based-natural-language-processing";
            SearchPatternsAndCheckExtractions(GetFileContent, UrlFieldsFileContent, text,
                ("Common.Url.Domain", new[] { "blog.nezaboodka.com" }),
                ("Common.Url.Path", new[] { "/post/2019/599-nevod-technology-for-pattern-based-natural-language-processing" })
            );
        }

        [TestMethod]
        public void SearchNevodPackageExtensionOnly()
        {
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: " +
                "https://blog.nezaboodka.com/post/2019/599-nevod-technology-for-pattern-based-natural-language-processing";
            SearchPatternsAndCheckMatches(GetFileContent, NevodExtensionFileContent, text,
                ("Nv.Nevod-in-minus", new[] { "-nevod-" }),
                ("Nv.Nevod", new[] { "Nevod", "nevod" })
            );
        }

        [TestMethod]
        public void SearchNevodPackageWithExtension()
        {
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: " +
                "https://blog.nezaboodka.com/post/2019/599-nevod-technology-for-pattern-based-natural-language-processing";
            SearchPatternsAndCheckMatches(GetFileContent, SearchNevodWithExtensionFileContent, text,
                ("Nv.Nezaboodka", new[] { "Nezaboodka", "nezaboodka" }),
                ("Nv.Nevod-in-minus", new[] { "-nevod-" }),
                ("Nv.Nevod", new[] { "Nevod", "nevod" })
            );
        }

        [TestMethod]
        public void SearchNevodPackageWithExtensionExplicitTarget()
        {
            string text = "Nezaboodka Software LLC provides Nevod technology. " +
                "Check out the company blog for latest news: " +
                "https://blog.nezaboodka.com/post/2019/599-nevod-technology-for-pattern-based-natural-language-processing";
            SearchPatternsAndCheckMatches(GetFileContent, SearchNevodWithExtensionExplicitTargetFileContent, text,
                ("Nv.Nezaboodka", new[] { "Nezaboodka", "nezaboodka" }),
                ("Nv.Nevod", new[] { "Nevod", "nevod" })
            );
        }

        [TestMethod]
        public void SearchPackage_RU()
        {
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(GetFileContent, UrlFileContent_RU, text,
                "www.google.com");
        }

        [TestMethod]
        public void SearchPatternsInRequiredPackages_RU()
        {
            string text = "Технологию Невод можно попробовать на сайте https://nevod.io/.";
            SearchPatternsAndCheckMatches(GetFileContent, SearchDomainFileContent_RU, text,
                "nevod.io");
        }

        private string GetFileContent(string filePath)
        {
            string result;
            string[] filePathParts = filePath.Split('/', '\\');
            if (filePathParts.Length > 0)
                filePath = filePathParts[filePathParts.Length - 1];
            switch (filePath)
            {
                case "Url.np":
                    result = UrlFileContent;
                    break;
                case "UrlFields.np":
                    result = UrlFieldsFileContent;
                    break;
                case "Contacts.np":
                    result = ContactsFileContent;
                    break;
                case "Nevod.np":
                    result = NevodFileContent;
                    break;
                case "NevodExtension.np":
                    result = NevodExtensionFileContent;
                    break;
                case "NevodExtensionExplicitTarget.np":
                    result = NevodExtensionExplicitTargetFileContent;
                    break;
                case "SearchContacts.np":
                    result = SearchContactsFileContent;
                    break;
                case "SearchDomain.np":
                    result = SearchDomainFileContent;
                    break;
                case "SearchUrlAndDomainDomain.np":
                    result = SearchUrlAndDomainFileContent;
                    break;
                case "Интернет-ссылка.невод":
                    result = UrlFileContent_RU;
                    break;
                default:
                    throw new ArgumentException($"{nameof(filePath)}: '{filePath}'");
            }
            return result;
        }

        const string UrlFileContent = @"
@namespace Common
{
    @pattern #Url = {'http', 'https'} + '://' + Domain + ?Path + ?Query
    @where
    {
        @pattern #Domain = Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
        @pattern Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
        @pattern Query = '?' + ?(Param + [0+ ('&' + Param)])
        @where
        {
            Param = Identifier + '=' + Identifier
            @where
            {
                @pattern Identifier = {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
            };
        };
    };
}
";
        const string UrlFieldsFileContent = @"
@namespace Common
{
    @search @pattern Url(Domain, Path, Query) = {'http', 'https'} + '://' + Domain:Url.Domain + Path:?Url.Path + Query:?Url.Query
    @where
    {
        @pattern #Domain = Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
        @pattern Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
        @pattern Query = '?' + ?(Param + [0+ ('&' + Param)])
        @where
        {
            Param = Identifier + '=' + Identifier
            @where
            {
                @pattern Identifier = {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
            };
        };
    };
}
";
        const string ContactsFileContent = @"
@require 'Url.np';
@namespace Common
{
    @pattern #PhoneNumber = ?'+' + {Num, '(' + Num + ')'} + [2+ ({'-', Space} + Num)];
    @pattern #Email = Word + [0+ {Word, '.', '_', '+', '-'}] + '@' + Url.Domain;
    @pattern #HashTag = '#' + Identifier;
    @pattern Identifier = {AlphaNum, Alpha, '_'} + [0+ ?'.' + {Word, '_'}];
}
";
        const string NevodFileContent = @"
@namespace Nv
{
    @pattern #Nevod = 'Nevod';
    @pattern #Nezaboodka = 'Nezaboodka';
}
";
        const string NevodExtensionFileContent = @"
@require 'Nevod.np';

@search Nv.Nevod;

@namespace Nv
{
    @search @pattern Nevod-in-minus = '-' + Nevod + '-';
}
";
        const string NevodExtensionExplicitTargetFileContent = @"
@require 'Nevod.np';

@search Nv.Nevod;
@search Nv.Nevod-in-minus;

@namespace Nv
{
    @pattern Nevod-in-minus = '-' + Nevod + '-';
}
";
        const string SearchContactsFileContent = @"
@require 'Contacts.np';
@require 'Url.np';
@search Common.PhoneNumber;
@search Common.Email;
@search Common.Url;
@search Common.Url.Domain;
";

        const string SearchDomainFileContent = @"
@require 'Url.np';
@search Common.Url.Domain;
";

        const string SearchUrlAndDomainFileContent = @"
@require 'Url.np';
@search Common.*;
";
        const string SearchNevodWithExtensionFileContent = @"
@require 'NevodExtension.np';
@require 'Nevod.np';

@search Nv.*;
";
        const string SearchNevodWithExtensionExplicitTargetFileContent = @"
@require 'NevodExtensionExplicitTarget.np';
@require 'Nevod.np';

@search Nv.*;
";

        const string UrlFileContent_RU = @"
@пространство Общее
{
    @шаблон #Интернет-ссылка = {'http', 'https'} + '://' + Домен + ?Путь + ?Запрос
    @где
    {
        @шаблон #Домен = Слово + [1+ ('.' + Слово + [0+ {Слово, '_', '-'}])];
        @шаблон Путь = '/' + [0+ {Слово, '/', '_', '+', '-', '%'}];
        @шаблон Запрос = '?' + ?(Параметр + [0+ ('&' + Параметр)])
        @где
        {
            Параметр = Идентификатор + '=' + Идентификатор
            @где
            {
                @шаблон Идентификатор = {БуквыЦифры, Буквы, '_'} + [0+ {Слово, '_'}];
            };
        };
    };
}
";
        const string SearchDomainFileContent_RU = @"
@требуется 'Интернет-ссылка.невод';
@искать Общее.Интернет-ссылка.Домен;
";
    }
}
