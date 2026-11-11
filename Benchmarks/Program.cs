/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using BenchmarkDotNet.Running;

// runs the benchmarks chosen on the command line, or asks which to run when there are none; for example:
//   dotnet run -c Release --project Benchmarks -- --filter '*'
//   dotnet run -c Release --project Benchmarks -- --filter '*Containment*' --job short
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
