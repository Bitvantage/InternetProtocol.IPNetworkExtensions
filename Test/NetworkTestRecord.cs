/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Net;

namespace Test;

/// <summary>
///     The expected properties of a network built from an address and a prefix length.
/// </summary>
public record NetworkTestRecord
{
    /// <summary>
    ///     Creates a test case from the text of each address.
    /// </summary>
    public NetworkTestRecord(string address, int prefixLength, string baseAddress, string networkMask, string broadcastAddress, string hostMask, string firstHost, string lastHost)
    {
        Address = IPAddress.Parse(address);
        PrefixLength = prefixLength;
        BaseAddress = IPAddress.Parse(baseAddress);
        NetworkMask = IPAddress.Parse(networkMask);
        BroadcastAddress = IPAddress.Parse(broadcastAddress);
        HostMask = IPAddress.Parse(hostMask);
        FirstHost = IPAddress.Parse(firstHost);
        LastHost = IPAddress.Parse(lastHost);
    }

    /// <summary>
    ///     The address that the network is built from, which may have host bits set.
    /// </summary>
    public IPAddress Address { get; init; }

    /// <summary>
    ///     The expected base address, which is <see cref="Address" /> with the host bits cleared.
    /// </summary>
    public IPAddress BaseAddress { get; init; }

    /// <summary>
    ///     The expected broadcast address.
    /// </summary>
    public IPAddress BroadcastAddress { get; init; }

    /// <summary>
    ///     The expected first host address.
    /// </summary>
    public IPAddress FirstHost { get; init; }

    /// <summary>
    ///     The expected host mask.
    /// </summary>
    public IPAddress HostMask { get; init; }

    /// <summary>
    ///     The expected last host address.
    /// </summary>
    public IPAddress LastHost { get; init; }

    /// <summary>
    ///     The expected network mask.
    /// </summary>
    public IPAddress NetworkMask { get; init; }

    /// <summary>
    ///     The prefix length that the network is built with.
    /// </summary>
    public int PrefixLength { get; init; }
}
