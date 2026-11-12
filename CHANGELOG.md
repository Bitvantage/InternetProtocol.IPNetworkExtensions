# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.1] - 2026-11-12

### Changed

- `IPNetwork.GetPrefixLengthForSubnets` gives a host network, /32 or /128, for an empty list of prefix lengths instead of throwing, as `GetPrefixLengthForAddresses` does for zero addresses. The host network holds the empty plan, which `AllocateSubnets` accepts.

## [1.1.0] - 2026-11-12

### Added

- `IPNetwork.GetPrefixLengthForAddresses`, the longest prefix length whose networks hold a number of addresses. It counts every address, where `GetPrefixLengthForHosts` leaves out the ones a host can't use, which differ between IPv4 and IPv6.
- `IPNetwork.GetPrefixLengthForSubnets`, which sizes a network for a number of subnets of one prefix length, such as 300 /64s, or for one subnet of each of several prefix lengths.
- `AllocateSubnets` and `TryAllocateSubnets`, which lay out a variable-length subnet masking (VLSM) plan inside a network, around any networks already allocated. Each subnet goes in the smallest free block that holds it, so the subnets fit whenever any arrangement of them fits, and a subnet never moves once it is placed. The subnets are returned in the order requested.
- `ContainsStrict`, which tests whether an address is in a network while keeping IPv4 and IPv6 apart, with the same answer on every version of .NET. The built-in `IPNetwork.Contains` maps IPv4-mapped IPv6 addresses to IPv4 from .NET 9 on, and on .NET 9 and .NET 10 also tests them wrongly against IPv6 networks, so that `fc00::/7` contains `::ffff:1.2.3.4`.
- `Unmap()` on a network, which converts an IPv4-mapped IPv6 network, such as `::ffff:10.0.0.0/104`, to the IPv4 network `10.0.0.0/8`.

### Fixed

- The migration guide mapped `NetworkPrefix.Contains(IPAddress)` to the built-in `IPNetwork.Contains`, but `NetworkPrefix` kept IPv4 and IPv6 apart and the built-in does not from .NET 9 on. It now maps to `ContainsStrict`, and the documentation no longer recommends the built-in `Contains` for IPv4-mapped addresses.

## [1.0.0] - 2026-11-11

First release. The library adds CIDR, subnet and supernet math to .NET's built-in `System.Net.IPNetwork` and
`IPAddress` as C# 14 extension members, for .NET 8 and .NET 10. It succeeds `Bitvantage.InternetProtocol.NetworkPrefix`
2.0.0; see [MIGRATION.md](MIGRATION.md) for moving from its `NetworkPrefix` class.

[1.1.1]: https://github.com/Bitvantage/InternetProtocol.IPNetworkExtensions/releases/tag/v1.1.1
[1.1.0]: https://github.com/Bitvantage/InternetProtocol.IPNetworkExtensions/releases/tag/v1.1.0
[1.0.0]: https://github.com/Bitvantage/InternetProtocol.IPNetworkExtensions/releases/tag/v1.0.0
