// See https://aka.ms/new-console-template for more information

using ParserObjects.BenchmarkHarness;

var output = BenchmarkDotNet.Running.BenchmarkRunner.Run<CaptureBenchmarks>();
