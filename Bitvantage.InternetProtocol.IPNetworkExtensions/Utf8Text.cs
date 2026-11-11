/*
   Bitvantage.InternetProtocol.IPNetworkExtensions
   Copyright (c) 2024-2026 Michael Crino
   
   Licensed under the MIT License. See LICENSE.md in the repository root for the full license text.
*/
using System.Buffers;
using System.Text;

namespace Bitvantage.InternetProtocol;

/// <summary>
///     Parses UTF-8 text with a parser that reads characters, by converting the text to characters first.
/// </summary>
internal static class Utf8Text
{
    /// <summary>
    ///     The longest UTF-8 text that is converted to characters on the stack rather than in a rented array. Every valid address
    ///     or network is far shorter, but text may be padded with whitespace, so longer text is still parsed.
    /// </summary>
    private const int StackLimit = 256;

    /// <summary>
    ///     Parses text that has been converted to characters.
    /// </summary>
    /// <typeparam name="TResult">The type of the parsed value.</typeparam>
    /// <param name="text">The text.</param>
    /// <param name="result">When this method returns <see langword="true" />, the parsed value.</param>
    /// <returns><see langword="true" /> when the text is valid.</returns>
    internal delegate bool CharacterParser<TResult>(ReadOnlySpan<char> text, out TResult result);

    /// <summary>
    ///     Converts UTF-8 text to characters and parses them.
    /// </summary>
    /// <typeparam name="TResult">The type of the parsed value.</typeparam>
    /// <param name="utf8Text">The UTF-8 text.</param>
    /// <param name="parser">The parser for the characters.</param>
    /// <param name="result">When this method returns <see langword="true" />, the parsed value.</param>
    /// <returns><see langword="true" /> when the text is valid; invalid UTF-8 is never valid.</returns>
    internal static bool TryParse<TResult>(ReadOnlySpan<byte> utf8Text, CharacterParser<TResult> parser, out TResult result)
    {
        // UTF-8 never needs more characters than it has bytes, so a buffer as long as the text always fits it. Short text, which
        // is all valid text, converts on the stack
        char[]? rentedCharacters = null;
        var characters = utf8Text.Length <= StackLimit
            ? stackalloc char[StackLimit]
            : rentedCharacters = ArrayPool<char>.Shared.Rent(utf8Text.Length);

        try
        {
            // invalid UTF-8 becomes replacement characters, which then fail to parse
            var characterCount = Encoding.UTF8.GetChars(utf8Text, characters);
            return parser(characters[..characterCount], out result);
        }
        finally
        {
            if (rentedCharacters is not null)
                ArrayPool<char>.Shared.Return(rentedCharacters);
        }
    }
}
