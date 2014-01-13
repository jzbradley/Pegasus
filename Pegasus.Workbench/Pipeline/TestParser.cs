﻿namespace Pegasus.Workbench.Pipeline
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using Pegasus.Common;

    public class TestParser
    {
        public TestParser(IObservable<dynamic> parsers, IObservable<string> subjects, IObservable<string> fileNames)
        {
            var testParserResults = subjects
                .CombineLatest(fileNames, (subject, fileName) => new { subject, fileName })
                .ObserveOn(Scheduler.Default)
                .CombineLatest(parsers, (p, parser) => new { p.subject, p.fileName, parser = (object)parser })
                .Select(p => ParseTest(p.parser, p.subject, p.fileName));

            this.Results = testParserResults.Select(r => r.Result);
            this.Errors = testParserResults.Select(r => r.Errors.AsReadOnly());
        }

        public IObservable<IList<CompilerError>> Errors { get; set; }

        public IObservable<object> Results { get; set; }

        private static ParseResult ParseTest(dynamic parser, string subject, string fileName)
        {
            subject = subject ?? "";

            try
            {
                return new ParseResult
                {
                    Result = parser.Parse(subject, fileName),
                    Errors = new List<CompilerError>(),
                };
            }
            catch (Exception ex)
            {
                var cursor = ex.Data["cursor"] as Cursor ?? new Cursor(subject, 0, fileName);

                var parts = Regex.Split(ex.Message, @"(?<=^\w+):");
                if (parts.Length == 1)
                {
                    parts = new[] { "", parts[0] };
                }

                return new ParseResult
                {
                    Errors = new List<CompilerError>
                    {
                        new CompilerError(fileName, cursor.Line, cursor.Column, errorNumber: parts[0], errorText: parts[1]),
                    },
                };
            }
        }

        private class ParseResult
        {
            public List<CompilerError> Errors { get; set; }

            public object Result { get; set; }
        }
    }
}
