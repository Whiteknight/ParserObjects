// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using ParserObjects.BenchmarkHarness;

//var benchmarks = new CaptureBenchmarks();
//benchmarks.RuleBasedParser();
//benchmarks.CaptureBasedParser();

//var output = BenchmarkRunner.Run<CaptureBenchmarks>();

var output = BenchmarkRunner.Run<StringSequenceBenchmarks>();
