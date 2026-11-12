/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Net;
using Bitvantage.InternetProtocol;
using static Test.TestNetworks;

namespace Test;

public class ContainmentTests
{
    [Test]
    public void IsSubnetOf_IPv4_01()
    {
        var largerNetwork = Network("131.192.0.0/10");

        Assert.That(largerNetwork.IsSupernetOf(Network("192.0.0.0/2")), Is.False);
        Assert.That(largerNetwork.IsSubnetOf(Network("192.0.0.0/2")), Is.False);
    }

    [Test]
    public void IsProperSubnetOf_IPv4_01()
    {
        var smallerNetwork = Network("10.20.30.0/24");

        Assert.That(smallerNetwork.IsProperSubnetOf(Network("10.20.0.0/16")), Is.True);
        Assert.That(smallerNetwork.IsProperSubnetOf(Network("10.20.30.0/24")), Is.False);
        Assert.That(smallerNetwork.IsProperSubnetOf(Network("10.20.30.0/25")), Is.False);
        Assert.That(smallerNetwork.IsProperSubnetOf(Network("10.21.0.0/16")), Is.False);

        Assert.That(smallerNetwork.IsSubnetOf(Network("10.20.0.0/16")), Is.True);
        Assert.That(smallerNetwork.IsSubnetOf(Network("10.20.30.0/24")), Is.True);
        Assert.That(smallerNetwork.IsSubnetOf(Network("10.20.30.0/25")), Is.False);
        Assert.That(smallerNetwork.IsSubnetOf(Network("10.21.0.0/16")), Is.False);
    }

    [Test]
    public void IsProperSubnetOf_IPv4_02()
    {
        // the subnet tests are the inverse of the supernet tests
        var largerNetwork = Network("10.20.0.0/16");
        var smallerNetwork = Network("10.20.30.0/24");

        Assert.That(smallerNetwork.IsProperSubnetOf(largerNetwork), Is.EqualTo(largerNetwork.IsProperSupernetOf(smallerNetwork)));
        Assert.That(largerNetwork.IsProperSubnetOf(smallerNetwork), Is.EqualTo(smallerNetwork.IsProperSupernetOf(largerNetwork)));
        Assert.That(smallerNetwork.IsSubnetOf(largerNetwork), Is.EqualTo(largerNetwork.IsSupernetOf(smallerNetwork)));
        Assert.That(largerNetwork.IsSubnetOf(smallerNetwork), Is.EqualTo(smallerNetwork.IsSupernetOf(largerNetwork)));
    }

    [Test]
    public void IsProperSupernetOf_IPv4_01()
    {
        Assert.That(Network("0.0.0.0/1").IsProperSupernetOf(Network("116.11.0.0/16")), Is.True);
    }

    [Test]
    public void IsProperSupernetOf_IPv4_02()
    {
        var largerNetwork = Network("10.20.0.0/16");

        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.20.0.0/16")), Is.False);
        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.20.0.0/24")), Is.True);
        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.20.255.0/24")), Is.True);
        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.20.30.40/32")), Is.True);
        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.21.0.0/24")), Is.False);
        Assert.That(largerNetwork.IsProperSupernetOf(Network("10.20.0.0/15")), Is.False);
    }

    [Test]
    public void ContainsAddress_IPv4_01()
    {
        // testing an address calls the built-in Contains, under which a host network contains the address it represents
        var hostNetwork = Network("10.20.30.40/32");

        Assert.That(hostNetwork.Contains(Address("10.20.30.40")), Is.True);
        Assert.That(hostNetwork.Contains(Address("10.20.30.41")), Is.False);

        var network = Network("10.20.30.0/24");

        Assert.That(network.Contains(Address("10.20.30.0")), Is.True);
        Assert.That(network.Contains(Address("10.20.30.255")), Is.True);
        Assert.That(network.Contains(Address("10.20.31.40")), Is.False);
    }

    [Test]
    public void ContainsStrict_01()
    {
        // an address is in a network exactly when its host network is a subnet of it, at every prefix length of both IP versions
        var addresses = new[] { "10.1.2.3", "0.0.0.0", "255.255.255.255", "::ffff:10.1.2.3", "::10.1.2.3", "2001:db8::a01:203", "::", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff" }.Select(Address).ToArray();

        foreach (var networkAddress in addresses)
        {
            var hostNetwork = IPNetwork.FromAddress(networkAddress);
            for (var prefixLength = 0; prefixLength <= hostNetwork.PrefixLength; prefixLength++)
            {
                var network = hostNetwork.GetSupernet(prefixLength);
                foreach (var address in addresses)
                    Assert.That(network.ContainsStrict(address), Is.EqualTo(network.IsSupernetOf(IPNetwork.FromAddress(address))), $"{network} and {address}");
            }
        }
    }

    [Test]
    public void ContainsStrict_IPv4_01()
    {
        var network = Network("10.20.30.0/24");

        Assert.That(network.ContainsStrict(Address("10.20.30.0")), Is.True);
        Assert.That(network.ContainsStrict(Address("10.20.30.255")), Is.True);
        Assert.That(network.ContainsStrict(Address("10.20.31.40")), Is.False);

        // a host network contains the address it represents
        Assert.That(Network("10.20.30.40/32").ContainsStrict(Address("10.20.30.40")), Is.True);
        Assert.That(Network("10.20.30.40/32").ContainsStrict(Address("10.20.30.41")), Is.False);

        // an address of the other IP version is never in the network, including an IPv4-mapped or IPv4-compatible one
        Assert.That(Network("10.0.0.0/8").ContainsStrict(Address("::ffff:10.1.2.3")), Is.False);
        Assert.That(Network("0.0.0.0/0").ContainsStrict(Address("::ffff:8.8.8.8")), Is.False);
        Assert.That(Network("0.0.0.0/0").ContainsStrict(IPAddress.IPv6Any), Is.False);
        Assert.That(Network("10.0.0.0/8").ContainsStrict(Address("::10.1.2.3")), Is.False);
        Assert.That(Network("10.0.0.0/8").ContainsStrict(Address("::ffff:10.1.2.3").Unmap()), Is.True);

        Assert.Throws<ArgumentNullException>(() => network.ContainsStrict(null!));
    }

    [Test]
    public void ContainsStrict_IPv6_01()
    {
        // the built-in Contains gets the first three wrong on .NET 9 and .NET 10, testing the mapped address as though the
        // networks were IPv4
        var mappedAddress = Address("::ffff:10.1.2.3");

        Assert.That(Network("::ffff:0:0/96").ContainsStrict(mappedAddress), Is.True);
        Assert.That(Network("::/8").ContainsStrict(mappedAddress), Is.True);
        Assert.That(Network("2001:db8::a01:200/120").ContainsStrict(mappedAddress), Is.False);

        Assert.That(Network("::ffff:0:0/96").ContainsStrict(Address("10.1.2.3")), Is.False);
        Assert.That(Network("::/0").ContainsStrict(Address("10.1.2.3")), Is.False);
        Assert.That(Network("2001:db8::/32").ContainsStrict(Address("2001:db8::1")), Is.True);
        Assert.That(Network("2001:db8::/32").ContainsStrict(Address("2001:db9::1")), Is.False);

        // a scope ID is not part of the address
        Assert.That(Network("fe80::/10").ContainsStrict(Address("fe80::1%3")), Is.True);
    }

    [Test]
    public void IsProperSupernetOf_IPv4_03()
    {
        // a network of the other IP version is never a subnet, and an address of the other version is never a member
        var everyIPv4Address = Network("0.0.0.0/0");

        Assert.That(everyIPv4Address.IsProperSupernetOf(Network("::/128")), Is.False);
        Assert.That(everyIPv4Address.IsSupernetOf(Network("::/0")), Is.False);
        Assert.That(everyIPv4Address.Contains(IPAddress.IPv6Any), Is.False);
    }

    [Test]
    public void IsSupernetOf_IPv4_02()
    {
        // the subnet tests keep IPv4 and IPv6 apart, while the built-in Contains maps an IPv4-mapped address to IPv4 from
        // .NET 9 on
        var network = Network("10.0.0.0/8");
        var mappedAddress = Address("::ffff:10.1.2.3");

        Assert.That(network.Contains(mappedAddress), Is.EqualTo(Environment.Version.Major >= 9));
        Assert.That(network.IsSupernetOf(IPNetwork.FromAddress(mappedAddress)), Is.False);
        Assert.That(network.IsSupernetOf(IPNetwork.FromAddress(mappedAddress.Unmap())), Is.True);
    }

    [Test]
    public void IsProperSupernetOf_IPv6_01()
    {
        var network = Network("2001:db8::/32");

        Assert.That(network.IsProperSupernetOf(Network("2001:db8:1::/48")), Is.True);
        Assert.That(network.IsProperSupernetOf(Network("2001:db9::/48")), Is.False);
        Assert.That(network.Contains(Address("2001:db8::1")), Is.True);
        Assert.That(network.Contains(Address("2001:db9::1")), Is.False);
    }

    [Test]
    public void IsSupernetOf_IPv4_01()
    {
        var largerNetwork = Network("10.20.0.0/16");

        Assert.That(largerNetwork.IsSupernetOf(Network("10.20.0.0/16")), Is.True);
        Assert.That(largerNetwork.IsSupernetOf(Network("10.20.0.0/24")), Is.True);
        Assert.That(largerNetwork.IsSupernetOf(Network("10.20.255.0/24")), Is.True);
        Assert.That(largerNetwork.IsSupernetOf(Network("10.20.30.40/32")), Is.True);
        Assert.That(largerNetwork.IsSupernetOf(Network("10.21.0.0/24")), Is.False);
        Assert.That(largerNetwork.IsSupernetOf(Network("10.20.0.0/15")), Is.False);
    }

    [Test]
    public void Overlaps_IPv4_01()
    {
        var network = Network("10.20.0.0/16");

        Assert.That(network.Overlaps(Network("10.20.0.0/16")), Is.True);
        Assert.That(network.Overlaps(Network("10.20.30.0/24")), Is.True);
        Assert.That(network.Overlaps(Network("10.0.0.0/8")), Is.True);
        Assert.That(network.Overlaps(Network("10.21.0.0/16")), Is.False);
        Assert.That(network.Overlaps(Network("::/0")), Is.False);
    }
}
