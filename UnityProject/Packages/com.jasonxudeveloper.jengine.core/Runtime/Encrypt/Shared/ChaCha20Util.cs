// ChaCha20Util.cs
//
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
//
//  Copyright (c) 2025 JEngine
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;

namespace JEngine.Core.Encrypt.Shared
{
    public static class ChaCha20Util
    {
        /// <summary>
        /// Encrypts data using ChaCha20 stream cipher
        /// </summary>
        public static byte[] ChaCha20Encrypt(byte[] data, byte[] key, byte[] nonce)
        {
            ValidateKeyAndNonce(key, nonce);
            return ChaCha20Transform(data, key, nonce);
        }

        /// <summary>
        /// Decrypts data using ChaCha20 stream cipher
        /// </summary>
        public static byte[] ChaCha20Decrypt(byte[] encryptedData, byte[] key, byte[] nonce)
        {
            ValidateKeyAndNonce(key, nonce);
            return ChaCha20Transform(encryptedData, key, nonce);
        }

        /// <summary>
        /// Decrypts file data using ChaCha20 stream cipher
        /// </summary>
        public static byte[] ChaCha20Decrypt(string filePath, byte[] key, byte[] nonce)
        {
            ValidateKeyAndNonce(key, nonce);
            byte[] fileData = File.ReadAllBytes(filePath);
            return ChaCha20Transform(fileData, key, nonce);
        }

        /// <summary>
        /// ChaCha20 transformation using RFC 7539 specification
        /// </summary>
        private static byte[] ChaCha20Transform(byte[] data, byte[] key, byte[] nonce)
        {
            byte[] result = new byte[data.Length];
            Span<uint> state = stackalloc uint[16];

            // Constants "expand 32-byte k"
            state[0] = 0x61707865;
            state[1] = 0x3320646e;
            state[2] = 0x79622d32;
            state[3] = 0x6b206574;

            // Key (little-endian)
            for (int i = 0; i < 8; i++)
            {
                state[4 + i] = BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(i * 4, 4));
            }

            // Initial counter
            state[12] = 0;

            // Nonce (little-endian)
            for (int i = 0; i < 3; i++)
            {
                state[13 + i] = BinaryPrimitives.ReadUInt32LittleEndian(nonce.AsSpan(i * 4, 4));
            }

            int offset = 0;
            uint blockCounter = 0;

            Span<uint> working = stackalloc uint[16];
            Span<byte> keystream = stackalloc byte[64];
            while (offset < data.Length)
            {
                state[12] = blockCounter;

                // Create working copy
                state.CopyTo(working);

                // 20 rounds (10 double rounds)
                for (int round = 0; round < 10; round++)
                {
                    // Column rounds
                    ChaCha20Quarter(working, 0, 4, 8, 12);
                    ChaCha20Quarter(working, 1, 5, 9, 13);
                    ChaCha20Quarter(working, 2, 6, 10, 14);
                    ChaCha20Quarter(working, 3, 7, 11, 15);

                    // Diagonal rounds
                    ChaCha20Quarter(working, 0, 5, 10, 15);
                    ChaCha20Quarter(working, 1, 6, 11, 12);
                    ChaCha20Quarter(working, 2, 7, 8, 13);
                    ChaCha20Quarter(working, 3, 4, 9, 14);
                }

                // Add original state
                for (int i = 0; i < 16; i++)
                {
                    working[i] += state[i];
                }

                // Generate keystream and XOR
                for (int i = 0; i < 16; i++)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(keystream.Slice(i * 4, 4), working[i]);
                }

                int blockSize = Math.Min(64, data.Length - offset);
                for (int i = 0; i < blockSize; i++)
                {
                    result[offset + i] = (byte)(data[offset + i] ^ keystream[i]);
                }

                offset += blockSize;
                blockCounter++;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ChaCha20Quarter(Span<uint> state, int a, int b, int c, int d)
        {
            state[a] += state[b];
            state[d] ^= state[a];
            state[d] = RotateLeft(state[d], 16);
            state[c] += state[d];
            state[b] ^= state[c];
            state[b] = RotateLeft(state[b], 12);
            state[a] += state[b];
            state[d] ^= state[a];
            state[d] = RotateLeft(state[d], 8);
            state[c] += state[d];
            state[b] ^= state[c];
            state[b] = RotateLeft(state[b], 7);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int bits)
        {
            return (value << bits) | (value >> (32 - bits));
        }

        /// <summary>
        /// Validates that key and nonce have correct sizes for ChaCha20
        /// </summary>
        private static void ValidateKeyAndNonce(byte[] key, byte[] nonce)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "ChaCha20 key cannot be null");
            if (nonce == null)
                throw new ArgumentNullException(nameof(nonce), "ChaCha20 nonce cannot be null");
            if (key.Length != 32)
                throw new ArgumentException($"ChaCha20 requires a 32-byte key, got {key.Length} bytes", nameof(key));
            if (nonce.Length != 12)
                throw new ArgumentException($"ChaCha20 requires a 12-byte nonce, got {nonce.Length} bytes",
                    nameof(nonce));
        }
    }
}