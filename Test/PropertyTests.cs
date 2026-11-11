/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Collections.ObjectModel;
using System.Net;
using System.Numerics;
using Bitvantage.InternetProtocol;
using static Test.TestNetworks;

namespace Test;

public class PropertyTests
{
    public static readonly ReadOnlyCollection<NetworkTestRecord> TestAddresses = new List<NetworkTestRecord>
    {
        new("10.0.0.0", 16, "10.0.0.0", "255.255.0.0", "10.0.255.255", "0.0.255.255", "10.0.0.1", "10.0.255.254"),
        new("10.0.2.3", 16, "10.0.0.0", "255.255.0.0", "10.0.255.255", "0.0.255.255", "10.0.0.1", "10.0.255.254"),
        new("10.0.2.0", 30, "10.0.2.0", "255.255.255.252", "10.0.2.3", "0.0.0.3", "10.0.2.1", "10.0.2.2"),
        new("10.0.2.3", 30, "10.0.2.0", "255.255.255.252", "10.0.2.3", "0.0.0.3", "10.0.2.1", "10.0.2.2"),
        new("10.0.2.3", 31, "10.0.2.2", "255.255.255.254", "10.0.2.3", "0.0.0.1", "10.0.2.2", "10.0.2.3"),
        new("10.0.2.3", 32, "10.0.2.3", "255.255.255.255", "10.0.2.3", "0.0.0.0", "10.0.2.3", "10.0.2.3"),
        new("10.0.2.3", 0, "0.0.0.0", "0.0.0.0", "255.255.255.255", "255.255.255.255", "0.0.0.1", "255.255.255.254")
    }.AsReadOnly();

    [Test]
    public void GetComplementaryNetwork_IPv4_01()
    {
        Assert.That(Network("10.0.10.0/24").GetComplementaryNetwork(), Is.EqualTo(Network("10.0.11.0/24")));
        Assert.That(Network("10.0.11.0/24").GetComplementaryNetwork(), Is.EqualTo(Network("10.0.10.0/24")));
        Assert.That(Network("10.0.0.0/16").GetComplementaryNetwork(), Is.EqualTo(Network("10.1.0.0/16")));
        Assert.That(Network("10.1.0.0/16").GetComplementaryNetwork(), Is.EqualTo(Network("10.0.0.0/16")));
    }

    [Test]
    public void GetComplementaryNetwork_IPv4_02()
    {
        Assert.Throws<InvalidOperationException>(() => Network("0.0.0.0/0").GetComplementaryNetwork());
    }

    [Test]
    public void Any_01()
    {
        // the names follow IPAddress.Any, which is IPv4, and IPAddress.IPv6Any
        Assert.That(IPNetwork.Any, Is.EqualTo(Network("0.0.0.0/0")));
        Assert.That(IPNetwork.IPv6Any, Is.EqualTo(Network("::/0")));
        Assert.That(IPNetwork.Any.IsSupernetOf(Network("10.0.0.0/8")), Is.True);
        Assert.That(IPNetwork.IPv6Any.IsSupernetOf(Network("10.0.0.0/8")), Is.False);
    }

    [Test]
    public void FirstAndLastHost_IPv4_01()
    {
        // a /0 has a first and last host like any other network
        var everyAddress = Network("0.0.0.0/0");

        Assert.That(everyAddress.FirstHost, Is.EqualTo(Address("0.0.0.1")));
        Assert.That(everyAddress.LastHost, Is.EqualTo(Address("255.255.255.254")));
    }

    [Test]
    public void FirstAndLastHost_IPv6_01()
    {
        // IPv6 has no broadcast address, so only the subnet-router anycast address, the network address, is not a host
        var network = Network("2001:db8::/64");

        Assert.That(network.FirstHost, Is.EqualTo(Address("2001:db8::1")));
        Assert.That(network.LastHost, Is.EqualTo(Address("2001:db8::ffff:ffff:ffff:ffff")));
        Assert.That(network.LastAddress, Is.EqualTo(Address("2001:db8::ffff:ffff:ffff:ffff")));
    }

    [Test]
    public void FirstAndLastHost_IPv6_02()
    {
        // both addresses of a point-to-point network are hosts
        var network = Network("2001:db8::/127");

        Assert.That(network.FirstHost, Is.EqualTo(Address("2001:db8::")));
        Assert.That(network.LastHost, Is.EqualTo(Address("2001:db8::1")));
    }

    [Test]
    public void Properties_IPv4_01()
    {
        // the properties of a network created by the IPNetwork constructor. It is given the base address, since before .NET 10
        // the constructor throws for an address with host bits set
        foreach (var testAddress in TestAddresses)
            AssertMatches(new IPNetwork(testAddress.BaseAddress, testAddress.PrefixLength), testAddress);
    }

    [Test]
    public void Properties_IPv4_02()
    {
        // the same networks written as an address and prefix length
        foreach (var testAddress in TestAddresses)
            AssertMatches(Network($"{testAddress.Address}/{testAddress.PrefixLength}"), testAddress);
    }

    [Test]
    public void Properties_IPv4_03()
    {
        // the same networks written as an address and mask, or created from an address and mask
        foreach (var testAddress in TestAddresses)
        {
            AssertMatches(Network($"{testAddress.Address} {testAddress.NetworkMask}"), testAddress);
            AssertMatches(IPNetwork.FromMask(testAddress.Address, testAddress.NetworkMask), testAddress);
        }
    }

    [Test]
    public void Totals_IPv4_01()
    {
        Assert.That(Network("10.0.0.0/24").TotalAddresses, Is.EqualTo(new BigInteger(256)));
        Assert.That(Network("10.0.0.0/24").TotalHosts, Is.EqualTo((UInt128)254));
        Assert.That(Network("10.0.0.0/31").TotalHosts, Is.EqualTo((UInt128)2));
        Assert.That(Network("10.0.0.0/32").TotalHosts, Is.EqualTo((UInt128)1));
        Assert.That(Network("0.0.0.0/0").TotalAddresses, Is.EqualTo(BigInteger.Pow(2, 32)));
        Assert.That(Network("0.0.0.0/0").TotalHosts, Is.EqualTo((UInt128)uint.MaxValue - 1));
    }

    [Test]
    public void Totals_IPv6_01()
    {
        // ::/0 holds 2^128 addresses, one more than a UInt128 can hold
        var everyAddress = Network("::/0");

        Assert.That(everyAddress.TotalAddresses, Is.EqualTo(BigInteger.Pow(2, 128)));

        // every address except the subnet-router anycast address is a host
        Assert.That(everyAddress.TotalHosts, Is.EqualTo(UInt128.MaxValue));
        Assert.That(Network("2001:db8::/64").TotalHosts, Is.EqualTo((UInt128)ulong.MaxValue));
        Assert.That(Network("2001:db8::/127").TotalHosts, Is.EqualTo((UInt128)2));
        Assert.That(Network("2001:db8::/128").TotalHosts, Is.EqualTo((UInt128)1));
    }

    [Test]
    public void Widths_IPv4_01()
    {
        var network = Network("10.0.0.0/24");

        Assert.That(network.AddressBitLength, Is.EqualTo(32));
        Assert.That(network.HostBitLength, Is.EqualTo(8));
        Assert.That(network.IsHostNetwork, Is.False);
        Assert.That(network.Version, Is.EqualTo(IPVersion.IPv4));
        Assert.That(network.BaseAddressBits, Is.EqualTo((UInt128)0x0A_00_00_00));
        Assert.That(network.NetworkMaskBits, Is.EqualTo((UInt128)0xFF_FF_FF_00));
        Assert.That(network.HostMaskBits, Is.EqualTo((UInt128)0xFF));
        Assert.That(Network("10.0.0.1").IsHostNetwork, Is.True);
    }

    [Test]
    public void Widths_IPv6_01()
    {
        var network = Network("2001:db8::/32");

        Assert.That(network.AddressBitLength, Is.EqualTo(128));
        Assert.That(network.HostBitLength, Is.EqualTo(96));
        Assert.That(network.Version, Is.EqualTo(IPVersion.IPv6));
        Assert.That(network.BaseAddressBits, Is.EqualTo(new UInt128(0x2001_0db8_0000_0000, 0)));
        Assert.That(network.NetworkMask, Is.EqualTo(Address("ffff:ffff::")));
        Assert.That(network.HostMask, Is.EqualTo(Address("::ffff:ffff:ffff:ffff:ffff:ffff")));
        Assert.That(Network("2001:db8::1").IsHostNetwork, Is.True);
    }

    /// <summary>
    ///     Asserts that a network has every property that a test case expects.
    /// </summary>
    private static void AssertMatches(IPNetwork network, NetworkTestRecord testAddress)
    {
        Assert.That(network.BaseAddress, Is.EqualTo(testAddress.BaseAddress));
        Assert.That(network.PrefixLength, Is.EqualTo(testAddress.PrefixLength));
        Assert.That(network.NetworkMask, Is.EqualTo(testAddress.NetworkMask));
        Assert.That(network.BroadcastAddress, Is.EqualTo(testAddress.BroadcastAddress));
        Assert.That(network.LastAddress, Is.EqualTo(testAddress.BroadcastAddress));
        Assert.That(network.HostMask, Is.EqualTo(testAddress.HostMask));
        Assert.That(network.FirstHost, Is.EqualTo(testAddress.FirstHost));
        Assert.That(network.LastHost, Is.EqualTo(testAddress.LastHost));
    }
}
