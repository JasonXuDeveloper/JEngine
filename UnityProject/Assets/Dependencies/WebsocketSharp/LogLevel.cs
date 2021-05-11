#region License
/*
 * LogLevel.cs
 *
 * The MIT License
 *
 * Copyright (c) 2013-2014 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

namespace WebSocketSharp
{
  /// <summary>
  /// Contains the values of the logging level.
  /// </summary>
  public enum LogLevel
  {
    /// <summary>
    /// Indicates the bottom logging level.
    /// </summary>
    Trace,
    /// <summary>
    /// Indicates the 2nd logging level from the bottom.
    /// </summary>
    Debug,
    /// <summary>
    /// Indicates the 3rd logging level from the bottom.
    /// </summary>
    Info,
    /// <summary>
    /// Indicates the 3rd logging level from the top.
    /// </summary>
    Warn,
    /// <summary>
    /// Indicates the 2nd logging level from the top.
    /// </summary>
    Error,
    /// <summary>
    /// Indicates the top logging level.
    /// </summary>
    Fatal
  }
}
