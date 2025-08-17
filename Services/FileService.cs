/*
 * ognp (OG Notepad)
 * A faithful Notepad clone written in C#.
 *
 * © 2025 Jim Sines (raystanza).
 *
 * Licensed under GPL-3.0-or-later.

 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;

namespace ognp.Services;

internal interface IFileService
{
    /// <summary>
    /// Load text from <paramref name="path"/> streaming with BOM detection.
    /// If the file is large (>= <paramref name="warnThresholdBytes"/>), calls
    /// <paramref name="confirmLargeFile"/> (if provided) to decide whether to proceed.
    /// </summary>
    /// <exception cref="FileNotFoundException"/>
    /// <exception cref="OperationCanceledException">When user cancels via confirmLargeFile.</exception>
    (string Text, Encoding Encoding) Load(
        string path,
        Func<long, bool>? confirmLargeFile = null,
        long warnThresholdBytes = 10L * 1024 * 1024);

    /// <summary>
    /// Save <paramref name="text"/> to <paramref name="path"/> using <paramref name="encoding"/>.
    /// Ensures the target directory exists. Flushes to disk.
    /// </summary>
    void Save(string path, string text, Encoding encoding);
}

internal sealed class FileService : IFileService
{
    public (string Text, Encoding Encoding) Load(
        string path,
        Func<long, bool>? confirmLargeFile = null,
        long warnThresholdBytes = 10L * 1024 * 1024)
    {
        var fi = new FileInfo(path);
        if (!fi.Exists)
            throw new FileNotFoundException("File not found.", path);

        if (warnThresholdBytes > 0 &&
            fi.Length >= warnThresholdBytes &&
            confirmLargeFile is not null &&
            !confirmLargeFile(fi.Length))
        {
            throw new OperationCanceledException("User canceled opening large file.");
        }

        // Stream with BOM detection; generous buffers + SequentialScan for speed.
        using var fs = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 1 << 16, options: FileOptions.SequentialScan);

        using var sr = new StreamReader(
            fs, Encoding.Default, detectEncodingFromByteOrderMarks: true, bufferSize: 1 << 16);

        var sb = new StringBuilder();
        var buf = new char[1 << 14];
        int n;
        while ((n = sr.Read(buf, 0, buf.Length)) > 0)
            sb.Append(buf, 0, n);

        return (sb.ToString(), sr.CurrentEncoding);
    }

    public void Save(string path, string text, Encoding encoding)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var sw = new StreamWriter(fs, encoding);
        sw.Write(text);
        sw.Flush();
        fs.Flush(true); // push to disk
    }
}
