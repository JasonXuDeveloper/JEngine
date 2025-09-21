// XorStream.cs
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
using System.IO;

namespace JEngine.Core.Encrypt
{
    /// <summary>
    /// xor解密流
    /// </summary>
    public class XorStream : FileStream
    {
        private readonly byte[] _key;

        public XorStream(byte[] key, string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode,
            access, share)
        {
            _key = key;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            // Get the current position before reading
            var currentPosition = Position;

            // Read the encrypted data
            var bytesRead = base.Read(array, offset, count);

            // Decrypt the data based on the file position, not buffer position
            Span<byte> span = array;
            for (int i = 0; i < bytesRead; i++)
            {
                int bufferIdx = offset + i;
                long filePos = currentPosition + i;
                span[bufferIdx] ^= _key[filePos % _key.Length];
            }

            return bytesRead;
        }
    }
}