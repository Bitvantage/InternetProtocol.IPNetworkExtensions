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
///     Properties and comparisons, which convert the network to integers on each call. The members that return an
///     <see cref="IPAddress" /> also create one.
/// </summary>
[MemoryDiagnoser]
public class PropertyBenchmarks
{
    private readonly IPNetwork ipv4Network = IPNetwork.Parse("10.20.0.0/16");
    private readonly IPNetwork ipv4Subnet = IPNetwork.Parse("10.20.30.0/24");
    private readonly IPNetwork ipv6Network = IPNetwork.Parse("2001:db8::/32");

    [Benchmark]
    public UInt128 BaseAddressBits_IPv4() => ipv4Network.BaseAddressBits;

    [Benchmark]
    public UInt128 BaseAddressBits_IPv6() => ipv6Network.BaseAddressBits;

    [Benchmark]
    public UInt128 TotalHosts_IPv4() => ipv4Network.TotalHosts;

    [Benchmark]
    public IPAddress NetworkMask_IPv4() => ipv4Network.NetworkMask;

    [Benchmark]
    public IPAddress BroadcastAddress_IPv4() => ipv4Network.BroadcastAddress;

    [Benchmark]
    public int CompareTo_IPv4() => ipv4Network.CompareTo(ipv4Subnet);

    [Benchmark]
    public AddressAllocation Allocation_IPv4() => ipv4Network.Allocation;

    [Benchmark]
    public AddressAllocation Allocation_IPv6() => ipv6Network.Allocation;
}
