/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Net;
using BenchmarkDotNet.Attributes;
using Bitvantage.InternetProtocol;

namespace Benchmarks;

/// <summary>
///     The strict parsers, measured against the built-in parsers, which accept nonstandard IPv4 forms.
/// </summary>
[MemoryDiagnoser]
public class ParsingBenchmarks
{
    private const string IPv4NetworkText = "192.168.100.0/24";
    private const string IPv6NetworkText = "2001:db8:1234:5678::/64";
    private const string IPv4AddressText = "192.168.100.200";

    private static readonly byte[] IPv4NetworkUtf8 = "192.168.100.0/24"u8.ToArray();

    [Benchmark(Baseline = true)]
    public IPNetwork BuiltInParse_IPv4Network() => IPNetwork.Parse(IPv4NetworkText);

    [Benchmark]
    public IPNetwork ParseStrict_IPv4Network() => IPNetwork.ParseStrict(IPv4NetworkText);

    [Benchmark]
    public IPNetwork ParseStrict_IPv4NetworkUtf8() => IPNetwork.ParseStrict(IPv4NetworkUtf8);

    [Benchmark]
    public IPNetwork ParseStrict_IPv4NetworkWithMask() => IPNetwork.ParseStrict("192.168.100.0 255.255.255.0");

    [Benchmark]
    public IPNetwork BuiltInParse_IPv6Network() => IPNetwork.Parse(IPv6NetworkText);

    [Benchmark]
    public IPNetwork ParseStrict_IPv6Network() => IPNetwork.ParseStrict(IPv6NetworkText);

    [Benchmark]
    public IPAddress BuiltInParse_IPv4Address() => IPAddress.Parse(IPv4AddressText);

    [Benchmark]
    public IPAddress ParseStrict_IPv4Address() => IPAddress.ParseStrict(IPv4AddressText);

    [Benchmark]
    public IReadOnlyList<IPNetwork> ParseRange_IPv4() => IPNetwork.ParseRange("10.0.0.1-10.0.255.254");
}
