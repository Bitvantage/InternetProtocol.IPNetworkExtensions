/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

namespace Bitvantage.InternetProtocol;

/// <summary>
///     The ways that the <c>ToString(NetworkFormat)</c> extension method on <see cref="System.Net.IPNetwork" /> can write a network.
/// </summary>
public enum NetworkFormat
{
    /// <summary>
    ///     The network address and the prefix length in CIDR notation, such as <c>10.0.0.0/24</c>. This is the default format.
    /// </summary>
    AddressAndPrefix,

    /// <summary>
    ///     A multi-line table of the network address, masks, broadcast address for IPv4, host range and host count, with each
    ///     address also written in binary.
    /// </summary>
    Detail,

    /// <summary>
    ///     The network address and the network mask separated by a space, such as <c>10.0.0.0 255.255.255.0</c>.
    /// </summary>
    AddressAndMask,

    /// <summary>
    ///     The network address alone, such as <c>10.0.0.0</c>.
    /// </summary>
    Address,

    /// <summary>
    ///     The network mask alone, such as <c>255.255.255.0</c>.
    /// </summary>
    Mask,

    /// <summary>
    ///     The network address and the host mask separated by a space, such as <c>10.0.0.0 0.0.0.255</c>. This is the form
    ///     that Cisco access control lists use, where the host mask is called a wildcard mask.
    /// </summary>
    /// <remarks>
    ///     The text parses back to the same network, except for a /0 or a /32 (or /128). Their host masks, all ones and all zeros,
    ///     are also valid network masks, and a parser reads them as network masks.
    /// </remarks>
    AddressAndHostMask,

    /// <summary>
    ///     The host mask alone, also called the wildcard mask, such as <c>0.0.0.255</c>.
    /// </summary>
    HostMask
}
