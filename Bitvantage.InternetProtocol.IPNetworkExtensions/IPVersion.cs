/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

namespace Bitvantage.InternetProtocol;

/// <summary>
///     A version of the Internet Protocol.
/// </summary>
public enum IPVersion
{
    /// <summary>
    ///     No version. A network always has a version other than this.
    /// </summary>
    Undefined = 0,

    /// <summary>
    ///     Internet Protocol version 4, with 32 bit addresses.
    /// </summary>
    IPv4 = 1,

    /// <summary>
    ///     Internet Protocol version 6, with 128 bit addresses.
    /// </summary>
    IPv6 = 2
}
