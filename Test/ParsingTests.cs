/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Net;
using System.Net.Sockets;
using Bitvantage.InternetProtocol;
using static Test.TestNetworks;

namespace Test;

public class ParsingTests
{
    [Test]
    public void FromAddress_01()
    {
        // a host network holds one address
        Assert.That(IPNetwork.FromAddress(Address("10.1.2.3")), Is.EqualTo(new IPNetwork(Address("10.1.2.3"), 32)));
        Assert.That(IPNetwork.FromAddress(Address("2001:db8::1")), Is.EqualTo(new IPNetwork(Address("2001:db8::1"), 128)));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.FromAddress(null!));
    }

    [Test]
    public void FromMask_01()
    {
        var address = Address("10.1.2.3");

        Assert.That(IPNetwork.FromMask(address, Address("255.255.0.0")), Is.EqualTo(Network("10.1.0.0/16")));

        // a mask with a gap, or a mask of the other IP version, is not a valid mask for the address
        Assert.Throws<ArgumentException>(() => IPNetwork.FromMask(address, Address("255.0.255.0")));
        Assert.Throws<ArgumentException>(() => IPNetwork.FromMask(address, Address("ffff::")));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.FromMask(address, null!));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.FromMask(null!, Address("255.0.0.0")));
    }

    [Test]
    public void FromHostMask_01()
    {
        var address = Address("10.1.2.3");

        Assert.That(IPNetwork.FromHostMask(address, Address("0.0.0.255")), Is.EqualTo(Network("10.1.2.0/24")));

        // as a host mask, all zeros is a host network and all ones is every address
        Assert.That(IPNetwork.FromHostMask(address, Address("0.0.0.0")), Is.EqualTo(Network("10.1.2.3/32")));
        Assert.That(IPNetwork.FromHostMask(address, Address("255.255.255.255")), Is.EqualTo(Network("0.0.0.0/0")));

        Assert.Throws<ArgumentException>(() => IPNetwork.FromHostMask(address, Address("0.0.255.0")));
        Assert.Throws<ArgumentException>(() => IPNetwork.FromHostMask(address, Address("::ff")));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.FromHostMask(address, null!));
    }

    [Test]
    public void GetMask_01()
    {
        Assert.That(IPNetwork.GetNetworkMask(IPVersion.IPv4, 24), Is.EqualTo(Address("255.255.255.0")));
        Assert.That(IPNetwork.GetHostMask(IPVersion.IPv4, 24), Is.EqualTo(Address("0.0.0.255")));
        Assert.That(IPNetwork.GetNetworkMask(IPVersion.IPv6, 32), Is.EqualTo(Address("ffff:ffff::")));
        Assert.That(IPNetwork.GetHostMask(IPVersion.IPv6, 128), Is.EqualTo(Address("::")));

        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetNetworkMask(IPVersion.IPv4, 33));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetNetworkMask(IPVersion.IPv4, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetHostMask(IPVersion.Undefined, 0));
    }

    [Test]
    public void Parse_HostMask_01()
    {
        // a mask that is not a network mask is read as a host mask, which is the form Cisco access control lists use
        Assert.That(IPNetwork.ParseStrict("10.0.0.0 0.0.0.255"), Is.EqualTo(Network("10.0.0.0/24")));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0/0.0.0.255"), Is.EqualTo(Network("10.0.0.0/24")));
        Assert.That(IPNetwork.ParseStrict("2001:db8:: ::ffff:ffff:ffff:ffff:ffff:ffff"), Is.EqualTo(Network("2001:db8::/32")));
        Assert.That(IPNetwork.TryParseStrict("10.0.0.0 0.0.255.0", out _), Is.False);

        // the all-zero and all-one masks are network masks
        Assert.That(IPNetwork.ParseStrict("10.0.0.0 0.0.0.0"), Is.EqualTo(Network("0.0.0.0/0")));
        Assert.That(IPNetwork.ParseStrict("10.0.0.1 255.255.255.255"), Is.EqualTo(Network("10.0.0.1/32")));
    }

    [Test]
    public void Parse_HostMask_02()
    {
        // a network written with its host mask parses back to the same network, except a /0 or /32, whose host masks are
        // also network masks
        for (var prefixLength = 1; prefixLength <= 31; prefixLength++)
        {
            var network = IPNetwork.FromAddress(Address("10.20.30.40")).GetSupernet(prefixLength);
            Assert.That(IPNetwork.ParseStrict(network.ToString(NetworkFormat.AddressAndHostMask)), Is.EqualTo(network), network.ToString());
        }

        Assert.That(IPNetwork.ParseStrict(Network("10.0.0.1/32").ToString(NetworkFormat.AddressAndHostMask)), Is.EqualTo(Network("0.0.0.0/0")));
    }

    [Test]
    public void Parse_Utf8_01()
    {
        Assert.That(IPNetwork.ParseStrict("10.0.0.0/24"u8), Is.EqualTo(Network("10.0.0.0/24")));
        Assert.That(IPNetwork.TryParseStrict("2001:db8:: ffff:ffff::"u8, out var network) && network == Network("2001:db8::/32"), Is.True);
        Assert.That(IPNetwork.TryParseStrict("010.0.0.0/8"u8, out _), Is.False);
        Assert.Throws<FormatException>(() => IPNetwork.ParseStrict("10.0.0.0/33"u8));

        // invalid UTF-8 fails to parse rather than throwing
        Assert.That(IPNetwork.TryParseStrict(new byte[] { 0x31, 0xff, 0x2e }, out _), Is.False);

        // text longer than the stack buffer, padded with whitespace, still parses
        var paddedText = System.Text.Encoding.UTF8.GetBytes(new string(' ', 300) + "10.0.0.0/8" + new string(' ', 300));
        Assert.That(IPNetwork.ParseStrict(paddedText), Is.EqualTo(Network("10.0.0.0/8")));
    }

    [Test]
    public void Parse_IPv4_01()
    {
        // host bits are cleared, so networks written with different host bits are equal
        var network = Network("10.1.2.3/8");

        Assert.That(network.BaseAddress, Is.EqualTo(Address("10.0.0.0")));
        Assert.That(network, Is.EqualTo(Network("10.0.0.0/8")));
        Assert.That(network.GetHashCode(), Is.EqualTo(Network("10.0.0.0/8").GetHashCode()));
    }

    [Test]
    public void Parse_IPv4_02()
    {
        // malformed text is rejected; TryParseStrict returns false instead of throwing, and ParseStrict throws a FormatException
        var malformedValues = new[]
        {
            "",
            "   ",
            "10.0.0.0/",
            "10.0.0.0/24/5",
            "10.0.0.0/33",
            "10.0.0.0/-1",
            "10.0.0.0/+8",
            "10.0.0.0/ 8 8",
            "10.0.0.0/4294967295",
            "10.0.0.0 255.0.255.0",
            "10.0.0.0 ffff::",
            "example.com/24",
            "10.0.0.0/24 garbage"
        };

        foreach (var malformedValue in malformedValues)
        {
            Assert.That(IPNetwork.TryParseStrict(malformedValue, out var network), Is.False, malformedValue);
            Assert.That(network, Is.EqualTo(default(IPNetwork)), malformedValue);
            Assert.Throws<FormatException>(() => IPNetwork.ParseStrict(malformedValue), malformedValue);
        }

        Assert.That(IPNetwork.TryParseStrict((string?)null, out _), Is.False);
        Assert.Throws<ArgumentNullException>(() => IPNetwork.ParseStrict((string)null!));
    }

    [Test]
    public void Parse_IPv4_03()
    {
        // surrounding whitespace is ignored, and an address may be separated from its mask by a slash or any run of whitespace
        var expected = Network("10.0.0.0/24");

        Assert.That(IPNetwork.ParseStrict(" 10.0.0.0/24 "), Is.EqualTo(expected));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0  255.255.255.0"), Is.EqualTo(expected));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0\t255.255.255.0"), Is.EqualTo(expected));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0/255.255.255.0"), Is.EqualTo(expected));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0/24".AsSpan()), Is.EqualTo(expected));
        Assert.That(IPNetwork.TryParseStrict("10.0.0.0/24".AsSpan(), out var parsedNetwork) && parsedNetwork == expected, Is.True);

        // a single address is a host network
        Assert.That(IPNetwork.ParseStrict("10.0.0.7"), Is.EqualTo(new IPNetwork(Address("10.0.0.7"), 32)));
    }

    [Test]
    public void Parse_IPv4_04()
    {
        // only the standard dotted-decimal form of IPv4 is accepted. IPAddress reads a leading zero as octal, so 010.0.0.0
        // would otherwise be 8.0.0.0, and it also accepts shorthand, whole-number and hexadecimal forms
        var nonstandardValues = new[]
        {
            "010.0.0.0/8",
            "10.0.0.010",
            "10.0.0/24",
            "10/8",
            "167772160/8",
            "0x0a.0.0.0/8",
            "10.0.0.0 255.255.255.00",
            "10.0.0.0/0255.255.255.0"
        };

        // Resolve rejects them too, rather than passing them to DNS, whose resolver could read them as an address its own way
        foreach (var nonstandardValue in nonstandardValues)
        {
            Assert.That(IPNetwork.TryParseStrict(nonstandardValue, out _), Is.False, nonstandardValue);
            Assert.Throws<FormatException>(() => IPNetwork.Resolve(nonstandardValue), nonstandardValue);
        }

        // five numbers is not an address in any form
        Assert.That(IPNetwork.TryParseStrict("10.0.0.0.0/8", out _), Is.False);

        // a lone zero is not a leading zero
        Assert.That(IPNetwork.ParseStrict("0.0.0.0/0"), Is.EqualTo(new IPNetwork(IPAddress.Any, 0)));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0 255.0.0.0"), Is.EqualTo(Network("10.0.0.0/8")));

        // IPv4 written inside IPv6 follows the IPv6 rules
        Assert.That(IPNetwork.ParseStrict("::ffff:10.0.0.0/104").Version, Is.EqualTo(IPVersion.IPv6));
    }

    [Test]
    public void Parse_IPv4_05()
    {
        // this is why ParseStrict exists: the built-in parser reads a leading zero as octal
        Assert.That(IPNetwork.Parse("010.0.0.0/8"), Is.EqualTo(IPNetwork.Parse("8.0.0.0/8")));
        Assert.That(IPNetwork.TryParseStrict("010.0.0.0/8", out _), Is.False);
    }

    [Test]
    public void Parse_IPv6_01()
    {
        Assert.That(IPNetwork.ParseStrict("2001:db8::1/64"), Is.EqualTo(new IPNetwork(Address("2001:db8::"), 64)));
        Assert.That(IPNetwork.ParseStrict("2001:db8:: ffff:ffff::"), Is.EqualTo(Network("2001:db8::/32")));
        Assert.That(IPNetwork.ParseStrict("2001:db8::1"), Is.EqualTo(Network("2001:db8::1/128")));
        Assert.That(IPNetwork.TryParseStrict("2001:db8::/129", out _), Is.False);
    }

    [Test]
    public void Parse_IPv6_02()
    {
        // a scope ID identifies an interface rather than part of an address, so it is dropped
        var network = IPNetwork.ParseStrict("fe80::1%3/64");

        Assert.That(network.BaseAddress.ScopeId, Is.Zero);
        Assert.That(network, Is.EqualTo(Network("fe80::/64")));
    }

    [Test]
    [Category("Network")]
    public void Resolve_01()
    {
        // text that is already a network is parsed without a lookup
        Assert.That(IPNetwork.Resolve("10.0.0.0/24"), Is.EqualTo(Network("10.0.0.0/24")));

        // an address with an invalid suffix is malformed, not a host name
        Assert.Throws<FormatException>(() => IPNetwork.Resolve("10.0.0.0/33"));
        Assert.Throws<FormatException>(() => IPNetwork.Resolve("one.one.one.one/"));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.Resolve("one.one.one.one", AddressFamily.AppleTalk));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.Resolve(null!));
    }

    [Test]
    [Category("Network")]
    public void Resolve_02()
    {
        // ParseStrict does not look up host names, which is a network operation; Resolve does
        Assert.Throws<FormatException>(() => IPNetwork.ParseStrict("one.one.one.one/32"));

        var network = IPNetwork.Resolve("one.one.one.one/32", AddressFamily.InterNetwork);
        Assert.That(network, Is.AnyOf(Network("1.1.1.1/32"), Network("1.0.0.1/32")));
    }

    [Test]
    [Category("Network")]
    public async Task Resolve_03()
    {
        // the IPv4 mask decides that an IPv4 address is looked up
        var network = await IPNetwork.ResolveAsync("one.one.one.one 255.255.255.255");
        Assert.That(network, Is.AnyOf(Network("1.1.1.1/32"), Network("1.0.0.1/32")));
    }

    [Test]
    public void Parse_IPv6_03()
    {
        // IPAddress reads endpoint text and drops the port, but the strict parser takes plain addresses only
        foreach (var networkText in new[] { "[2001:db8::]/32", "[2001:db8::]:80/32", "::1 [ffff::]", "[::1]" })
            Assert.That(IPNetwork.TryParseStrict(networkText, out _), Is.False, networkText);
    }

    [Test]
    public async Task Resolve_04()
    {
        // text that is already a network is checked against the IP version that was asked for, as a lookup would be
        Assert.That(IPNetwork.Resolve("10.0.0.0/8", AddressFamily.InterNetwork), Is.EqualTo(Network("10.0.0.0/8")));
        Assert.That(IPNetwork.Resolve("2001:db8::/32", AddressFamily.InterNetworkV6), Is.EqualTo(Network("2001:db8::/32")));
        Assert.Throws<FormatException>(() => IPNetwork.Resolve("10.0.0.0/8", AddressFamily.InterNetworkV6));
        await Assert.ThrowsAsync<FormatException>(() => IPNetwork.ResolveAsync("2001:db8::/32", AddressFamily.InterNetwork));

        // a malformed prefix length is rejected before the host name is looked up, so no lookup fails first
        foreach (var networkText in new[] { "host.invalid/024", "host.invalid/ 24", "host.invalid/129", "host.invalid 255.0.255.0" })
            Assert.Throws<FormatException>(() => IPNetwork.Resolve(networkText), networkText);
    }

    [Test]
    public void Parse_Strictness_01()
    {
        // a prefix length with a leading zero, or with whitespace after the slash, is rejected, as Go's netip does
        foreach (var networkText in new[] { "10.0.0.0/024", "10.0.0.0/00", "10.0.0.0/ 24", "10.0.0.0 /24", "2001:db8::/032" })
            Assert.That(IPNetwork.TryParseStrict(networkText, out _), Is.False, networkText);

        Assert.That(IPNetwork.ParseStrict("10.0.0.0/0"), Is.EqualTo(Network("0.0.0.0/0")));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0   255.0.0.0"), Is.EqualTo(Network("10.0.0.0/8")));
        Assert.That(IPNetwork.ParseStrict("10.0.0.0\t0.255.255.255"), Is.EqualTo(Network("10.0.0.0/8")));
    }

    [Test]
    public void ParseInterface_01()
    {
        var (ipv4Address, ipv4Network) = IPNetwork.ParseInterface("10.0.0.7/24");
        Assert.That(ipv4Address, Is.EqualTo(Address("10.0.0.7")));
        Assert.That(ipv4Network, Is.EqualTo(Network("10.0.0.0/24")));

        // every format that ParseStrict accepts, including Cisco's address and mask
        Assert.That(IPNetwork.ParseInterface("10.0.0.7 255.255.255.0"), Is.EqualTo((Address("10.0.0.7"), Network("10.0.0.0/24"))));
        Assert.That(IPNetwork.ParseInterface("10.0.0.7 0.0.0.255"u8), Is.EqualTo((Address("10.0.0.7"), Network("10.0.0.0/24"))));
        Assert.That(IPNetwork.ParseInterface("10.0.0.7".AsSpan()), Is.EqualTo((Address("10.0.0.7"), Network("10.0.0.7/32"))));

        // the address keeps a scope ID, and the network does not
        var (scopedAddress, scopedNetwork) = IPNetwork.ParseInterface("fe80::7%3/64");
        Assert.That(scopedAddress.ScopeId, Is.EqualTo(3));
        Assert.That(scopedNetwork, Is.EqualTo(Network("fe80::/64")));
    }

    [Test]
    public void ParseInterface_02()
    {
        Assert.That(IPNetwork.TryParseInterface("2001:db8::7/64", out var address, out var network), Is.True);
        Assert.That(address, Is.EqualTo(Address("2001:db8::7")));
        Assert.That(network, Is.EqualTo(Network("2001:db8::/64")));

        Assert.That(IPNetwork.TryParseInterface("10.0.0.7/24"u8, out address, out network), Is.True);
        Assert.That((address, network), Is.EqualTo((Address("10.0.0.7"), Network("10.0.0.0/24"))));

        foreach (var interfaceText in new[] { "010.0.0.7/24", "10.0.0.7/33", "10.0.0.7 255.0.255.0", "", "host/24" })
        {
            Assert.That(IPNetwork.TryParseInterface(interfaceText, out address, out network), Is.False, interfaceText);
            Assert.That(address, Is.Null);
            Assert.Throws<FormatException>(() => IPNetwork.ParseInterface(interfaceText), interfaceText);
        }

        Assert.That(IPNetwork.TryParseInterface((string?)null, out _, out _), Is.False);
        Assert.Throws<ArgumentNullException>(() => IPNetwork.ParseInterface((string)null!));
    }

    [Test]
    public void GetPrefixLengthForAddresses_01()
    {
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 256), Is.EqualTo(24));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 257), Is.EqualTo(23));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 2), Is.EqualTo(31));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 1), Is.EqualTo(32));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 0), Is.EqualTo(32));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, (UInt128)uint.MaxValue + 1), Is.EqualTo(0));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv6, 256), Is.EqualTo(120));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv6, UInt128.One << 64), Is.EqualTo(64));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv6, (UInt128.One << 127) + 1), Is.EqualTo(0));
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv6, UInt128.MaxValue), Is.EqualTo(0));

        // hosts leave out the reserved addresses, which differ between the IP versions, where addresses count every one
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 256), Is.EqualTo(23));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv6, 256), Is.EqualTo(119));

        // a network's own address count gives back its prefix length; only ::/0, with 2^128 addresses, doesn't fit a UInt128
        for (var prefixLength = 0; prefixLength <= 32; prefixLength++)
            Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, (UInt128)new IPNetwork(IPAddress.Any, prefixLength).TotalAddresses), Is.EqualTo(prefixLength));

        for (var prefixLength = 1; prefixLength <= 128; prefixLength++)
            Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv6, (UInt128)new IPNetwork(IPAddress.IPv6Any, prefixLength).TotalAddresses), Is.EqualTo(prefixLength));

        // an int variable compiles, as well as a constant
        var addressCount = 256;
        Assert.That(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, addressCount), Is.EqualTo(24));

        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, (UInt128)uint.MaxValue + 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForAddresses(IPVersion.Undefined, 1));
    }

    [Test]
    public void GetPrefixLengthForHosts_01()
    {
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 50), Is.EqualTo(26));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 62), Is.EqualTo(26));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 63), Is.EqualTo(25));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 2), Is.EqualTo(31));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 1), Is.EqualTo(32));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, 0), Is.EqualTo(32));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv6, 1000), Is.EqualTo(118));
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv6, UInt128.MaxValue), Is.EqualTo(0));

        // an int variable compiles, as well as a constant
        var hostCount = 254;
        Assert.That(IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, hostCount), Is.EqualTo(24));

        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, (UInt128)uint.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForHosts(IPVersion.IPv4, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForHosts(IPVersion.Undefined, 1));
    }

    [Test]
    public void GetPrefixLengthForSubnets_01()
    {
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, 64, 300), Is.EqualTo(55));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, 48, 200), Is.EqualTo(40));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 24, 16), Is.EqualTo(20));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 24, 17), Is.EqualTo(19));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 24, 1), Is.EqualTo(24));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 24, 0), Is.EqualTo(24));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 8, 256), Is.EqualTo(0));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, 128, UInt128.MaxValue), Is.EqualTo(0));

        // the two numbers swapped are just as valid, and give a different answer, which is what the parameter names are for
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, subnetPrefixLength: 16, subnetCount: 24), Is.EqualTo(11));

        // an int variable compiles, as well as a constant
        var subnetCount = 300;
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, 64, subnetCount), Is.EqualTo(55));

        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 8, 257))?.ParamName, Is.EqualTo("subnetCount"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 33, 1))?.ParamName, Is.EqualTo("subnetPrefixLength"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, -1, 1))?.ParamName, Is.EqualTo("subnetPrefixLength"));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, 24, -1))?.ParamName, Is.EqualTo("subnetCount"));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.Undefined, 24, 1));
    }

    [Test]
    public void GetPrefixLengthForSubnets_02()
    {
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [23, 24, 26]), Is.EqualTo(22));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [24, 24, 24]), Is.EqualTo(22));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [24, 24, 24, 24]), Is.EqualTo(22));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [25]), Is.EqualTo(25));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [1, 1]), Is.EqualTo(0));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, [64, 64, 64, 56]), Is.EqualTo(55));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, [0]), Is.EqualTo(0));

        // subnets of one length give the same answer as their count does
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, Enumerable.Repeat(64, 300)), Is.EqualTo(55));

        // an empty list gives a host network, as zero addresses do, and the host network holds the empty plan
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, []), Is.EqualTo(32));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, []), Is.EqualTo(128));
        Assert.That(IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, []), Is.EqualTo(IPNetwork.GetPrefixLengthForAddresses(IPVersion.IPv4, 0)));
        Assert.That(Network("10.0.0.7/32").AllocateSubnets([]), Is.Empty);

        // more addresses than the IP version has, and a prefix length that is out of range, are rejected
        Assert.Throws<ArgumentException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv6, [0, 128]));
        Assert.Throws<ArgumentException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [0, 0]));
        Assert.That(Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, [24, 33]))?.ParamName, Is.EqualTo("subnetPrefixLengths"));
        Assert.Throws<ArgumentOutOfRangeException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.Undefined, [24]));
        Assert.Throws<ArgumentNullException>(() => IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, null!));
    }

    [Test]
    public void GetPrefixLengthForSubnets_03()
    {
        // the subnets always fit in an empty network of the size given, and never in one half as large
        var random = new Random(20261113);

        for (var trial = 0; trial < 1000; trial++)
        {
            var prefixLengths = Enumerable.Range(0, random.Next(1, 20)).Select(_ => random.Next(8, 33)).ToArray();
            var prefixLength = IPNetwork.GetPrefixLengthForSubnets(IPVersion.IPv4, prefixLengths);
            var description = $"/{string.Join(", /", prefixLengths)} in a /{prefixLength}";

            Assert.That(new IPNetwork(IPAddress.Any, prefixLength).TryAllocateSubnets(prefixLengths, out _), Is.True, description);
            if (prefixLength < prefixLengths.Min())
                Assert.That(new IPNetwork(IPAddress.Any, prefixLength + 1).TryAllocateSubnets(prefixLengths, out _), Is.False, description);
        }
    }
}
