/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

namespace Bitvantage.InternetProtocol;

/// <summary>
///     The historical classes of the IPv4 address space, from before classless inter-domain routing.
/// </summary>
/// <remarks>
///     An IPv4-mapped IPv6 address or network, inside <c>::ffff:0:0/96</c>, takes the class of the IPv4 address or network that it
///     maps.
/// </remarks>
public enum NetworkClass
{
    /// <summary>
    ///     The network is an IPv6 network, or spans more than one class.
    /// </summary>
    Undefined = 0,

    /// <summary>
    ///     Class A, <c>0.0.0.0/1</c>.
    /// </summary>
    A = 1,

    /// <summary>
    ///     Class B, <c>128.0.0.0/2</c>.
    /// </summary>
    B = 2,

    /// <summary>
    ///     Class C, <c>192.0.0.0/3</c>.
    /// </summary>
    C = 3,

    /// <summary>
    ///     Class D, <c>224.0.0.0/4</c>, which is used for multicast.
    /// </summary>
    D = 4,

    /// <summary>
    ///     Class E, <c>240.0.0.0/4</c>, which is reserved.
    /// </summary>
    E = 5
}
