//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nezaboodka.Text.Parsing;
using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Telemetry")]
    public class SearchEngineTelemetryTests
    {
        [TestMethod]
        public void ShouldCollectTelemetryWhenOptionsFlagIsSet()
        {
            string patterns = "#P1 = [1+ '1 2' + Space];" +
                              "#P2 = '3 4';";
            string text = "1 2 1 2 3 4";

            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, new SearchOptions { CollectTelemetry = true });
            engine.Search(text);

            IReadOnlyCollection<CandidateInfo> telemetry = engine.GetTelemetry().Telemetry;
            telemetry.Should().HaveCount(6)
                .And.ContainEquivalentOf(new
                {
                    PatternId = 0,
                    StartTokenNumber = 1,
                    EndTokenNumber = 4,
                    CreationTokenNumber = 1
                }).And.ContainEquivalentOf(new
                {
                    PatternId = 0,
                    StartTokenNumber = 1,
                    EndTokenNumber = 8,
                    CreationTokenNumber = 4
                }).And.ContainEquivalentOf(new
                {
                    PatternId = 1,
                    StartTokenNumber = 9,
                    EndTokenNumber = 11,
                    CreationTokenNumber = 9
                });
        }

        [TestMethod]
        public void ShouldNotCollectTelemetryWhenOptionsFlagIsNotSet()
        {
            string patterns = "#P1 = [1+ '1 2' + Space];" +
                              "#P2 = '3 4';";
            string text = "1 2 1 2 3 4";

            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, new SearchOptions());
            engine.Search(text);

            IReadOnlyCollection<CandidateInfo> telemetry = engine.GetTelemetry().Telemetry;
            telemetry.Should().BeEmpty();
        }

        [TestMethod]
        public void ShouldTrackCandidateLimitExceeded()
        {
            string patterns = "#P = [1+ '1 2' + Space];";
            string text = "1 2 1 2 1 2";

            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, new SearchOptions
            {
                CollectTelemetry = true,
                CandidateLimit = 1
            });
            ParsedText parsedText = engine.GetParsedText(text);
            engine.Search(parsedText);

            IReadOnlyCollection<CandidateInfo> telemetry = engine.GetTelemetry().Telemetry;

            IEnumerable<int> candidateCountByTokenNumber = Enumerable.Range(0, parsedText.PlainTextTokens.Count)
                .Select(tokenNumber => telemetry.Count(x => tokenNumber >= x.StartTokenNumber && tokenNumber < x.EndTokenNumber));
            candidateCountByTokenNumber.Should().OnlyContain(x => x <= 1);
            telemetry.Should().OnlyContain(x => x.TextSourceId == 0);
        }

        [TestMethod]
        public void ShouldTrackMultipleTextSources()
        { 
            string patterns = "#P = '1 2';";
            string text1 = "1 2 1 2";
            string text2 = "1 2 1 2 1 2";

            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, new SearchOptions { CollectTelemetry = true });
            engine.Search(text1);
            engine.Search(text2);

            IReadOnlyCollection<CandidateInfo> telemetry = engine.GetTelemetry().Telemetry;
            telemetry.Should().HaveCount(5);
            telemetry.Where(x => x.TextSourceId == 0).Should().HaveCount(2);
            telemetry.Where(x => x.TextSourceId == 1).Should().HaveCount(3);
        }

        private class CsvModel
        {
            [Name("cid")]
            public long CandidateId { get; set; }
            [Name("pid")]
            public int PatternId { get; set; }
            [Name("tid")]
            public int TextSourceId { get; set; }

            [Name("sn")]
            public long StartTokenNumber { get; set; }
            [Name("en")]
            public long EndTokenNumber { get; set; }
            [Name("cn")]
            public long CreationTokenNumber { get; set; }
        }
        
        [TestMethod]
        public void ShouldExportTelemetryToCsv()
        {
            string patterns = "#P = '1 2';";
            string text1 = "1 2";
            string text2 = "1 2";

            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, new SearchOptions { CollectTelemetry = true });
            engine.Search(text1);
            engine.Search(text2);
            SearchEngineTelemetry telemetry = engine.GetTelemetry();
            string csv = telemetry.ToCsv();
            
            using var stringReader = new StringReader(csv);
            using var csvReader = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture));
            IEnumerable<CsvModel> records = csvReader.GetRecords<CsvModel>().ToArray();
            records.Should().BeEquivalentTo(
                new CsvModel
                {
                    CandidateId = 0,
                    PatternId = 0,
                    TextSourceId = 0,
                    StartTokenNumber = 1,
                    EndTokenNumber = 3,
                    CreationTokenNumber = 1
                },
                new CsvModel
                {
                    CandidateId = 1,
                    PatternId = 0,
                    TextSourceId = 1,
                    StartTokenNumber = 1,
                    EndTokenNumber = 3,
                    CreationTokenNumber = 1
                }
            );
        }
    }
}