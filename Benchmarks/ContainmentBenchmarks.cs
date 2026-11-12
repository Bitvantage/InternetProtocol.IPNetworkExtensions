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
///     Address, subnet, supernet and overlap tests, measured against the built-in test of whether a network contains an address.
/// </summary>
[MemoryDiagnoser]
public class ContainmentBenchmarks
{
    private readonly IPAddress ipv4Address = IPAddress.Parse("10.20.30.40");
    private readonly IPNetwork ipv4Network = IPNetwork.Parse("10.20.0.0/16");
    private readonly IPNetwork ipv4Subnet = IPNetwork.Parse("10.20.30.0/24");
    private readonly IPAddress ipv6Address = IPAddress.Parse("2001:db8:1::1");
    private readonly IPNetwork ipv6Network = IPNetwork.Parse("2001:db8::/32");
    private readonly IPNetwork ipv6Subnet = IPNetwork.Parse("2001:db8:1::/48");

    [Benchmark(Baseline = true)]
    public bool BuiltInContainsAddress_IPv4() => ipv4Network.Contains(ipv4Address);

    [Benchmark]
    public bool ContainsStrict_IPv4() => ipv4Network.ContainsStrict(ipv4Address);

    [Benchmark]
    public bool ContainsStrict_IPv6() => ipv6Network.ContainsStrict(ipv6Address);

    [Benchmark]
    public bool IsSupernetOf_IPv4() => ipv4Network.IsSupernetOf(ipv4Subnet);

    [Benchmark]
    public bool IsSupernetOf_IPv6() => ipv6Network.IsSupernetOf(ipv6Subnet);

    [Benchmark]
    public bool IsProperSubnetOf_IPv4() => ipv4Subnet.IsProperSubnetOf(ipv4Network);

    [Benchmark]
    public bool Overlaps_IPv4() => ipv4Subnet.Overlaps(ipv4Network);

    [Benchmark]
    public bool Overlaps_IPv6() => ipv6Subnet.Overlaps(ipv6Network);
}
