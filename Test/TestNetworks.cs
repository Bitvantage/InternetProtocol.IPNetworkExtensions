/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/

using System.Net;
using Bitvantage.InternetProtocol;

namespace Test;

/// <summary>
///     Shorthand for creating the networks and addresses that the tests use.
/// </summary>
internal static class TestNetworks
{
    /// <summary>
    ///     Parses an address.
    /// </summary>
    public static IPAddress Address(string addressText)
    {
        return IPAddress.Parse(addressText);
    }

    /// <summary>
    ///     Parses a network with the strict parser, which accepts every format the tests use.
    /// </summary>
    public static IPNetwork Network(string networkText)
    {
        return IPNetwork.ParseStrict(networkText);
    }

    /// <summary>
    ///     Parses several networks with the strict parser.
    /// </summary>
    public static IPNetwork[] Networks(params string[] networkTexts)
    {
        return networkTexts.Select(networkText => IPNetwork.ParseStrict(networkText)).ToArray();
    }
}
