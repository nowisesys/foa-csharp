using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FOA
{
    public enum SpecialChars
    {
        StartObject = '(', 
        StartArray = '[',
        EndObject = ')', 
        EndArray = ']'
    }

    public enum Option
    {
        EnableEscape
    }

    /// <summary>
    /// This class implements the encoder of FOA.
    /// </summary>
    public class Encoder
    {
        private Encoding encoding;   // Output encoding.
        private Stream stream;       // Destination stream.
        private byte[] buffer;       // Output buffer.
        private bool escape = true;  // Enable/disable escape sequences.

        /// <summary>
        /// This constructs an encoder with no backing store (the current stream
        /// is a bit bucket). Write() will still put the encoded data to the 
        /// buffer that can be retreived by calling GetBuffer().
        /// </summary>
        public Encoder()
        {
            SetStream(Stream.Null);
        }

        /// <summary>
        /// Creates an FOA encoding object.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public Encoder(Stream stream)
        {
            SetStream(stream);
        }

        /// <summary>
        /// Creates an FOA encoding object using the requested encoding.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        /// <param name="encoding">The encoding to use.</param>
        public Encoder(Stream stream, Encoding encoding)
        {
            SetStream(stream, encoding);
        }

        /// <summary>
        /// Set the target stream for encoding operations. Use Stream.Null as
        /// argument to disable backing store, see the default constructor.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public void SetStream(Stream stream)
        {
            this.stream = stream;
            SetEncoding();
        }

        /// <summary>
        /// Set the target stream for encoding operations.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        /// <param name="encoding">The encoding to use.</param>
        public void SetStream(Stream stream, Encoding encoding)
        {
            this.stream = stream;
            this.encoding = encoding;
        }

        /// <summary>
        /// Get active encoding.
        /// </summary>
        /// <returns>The active encoding.</returns>
        public Encoding GetEncoding()
        {
            return encoding;
        }

        /// <summary>
        /// Try to detect what encoding to use based on the type of stream
        /// currently in use.
        /// </summary>
        private void SetEncoding()
        {
            if (stream is MemoryStream)
            {
                encoding = Encoding.Unicode;
            }
            else
            {
                encoding = Encoding.UTF8;
            }
        }

        /// <summary>
        /// Explicit override the current encoding. Normally, the encoding is set
        /// automatic based on the type of stream set by SetStream(). The default
        /// are Unicode for memory streams and UTF8 for file streams.
        /// </summary>
        /// <param name="encoding">The encoding to use.</param>
        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// Enable or disable an option.
        /// </summary>
        /// <param name="option">The option to enable or disable.</param>
        /// <param name="value">Enable option if true.</param>
        public void SetOption(Option option, bool value)
        {
            if (option == Option.EnableEscape)
            {
                escape = value;
            }
        }

        /// <summary>
        /// Get an boolean value describing whether this option is enabled or
        /// disabled. Throws an ArgumentException if option is invalid (should 
        /// have been trapped by the compiler).
        /// </summary>
        /// <param name="option">The option to get the value for.</param>
        /// <returns>True if option is enabled.</returns>
        public bool GetOption(Option option)
        {
            if (option == Option.EnableEscape)
            {
                return escape;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Get current destination stream.
        /// </summary>
        /// <returns>The current used stream.</returns>
        public Stream GetStream()
        {
            return stream;
        }

        /// <summary>
        /// Get the write buffer. The buffer will contain the content of the
        /// last write operation.
        /// </summary>
        /// <returns>The byte array buffer.</returns>
        public byte[] GetBuffer()
        {
            return buffer;
        }

        /// <summary>
        /// Get content of write buffer as an string. The string is formatted
        /// using the current active encoding.
        /// </summary>
        /// <returns>The write buffer as an string object.</returns>
        public string GetString()
        {
            return encoding.GetString(buffer);
        }

        /// <summary>
        /// Write an entity to current destination stream. The argument should be an 
        /// string representing either an special char or data entity, possibly named.
        /// </summary>
        /// <param name="str">The string to write.</param>
        private void WriteEntity(string str)
        {
            buffer = encoding.GetBytes(str + "\n");
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes an anonymous data entity to current destination stream.
        /// </summary>
        /// <param name="value">The unnamed data.</param>
        public void Write(string value)
        {
            WriteEntity(GetEscaped(value));
        }

        /// <summary>
        /// Writes an named data entity to current destination stream.
        /// </summary>
        /// <param name="name">The data entity name.</param>
        /// <param name="value">The data entity value.</param>
        public void Write(string name, string value)
        {
            WriteEntity(name + " = " + GetEscaped(value));
        }

        /// <summary>
        /// Writes an named data entity to current destination stream.
        /// </summary>
        /// <param name="name">The data entity name.</param>
        /// <param name="value">The data entity value.</param>
        public void Write(string name, int value)
        {
            WriteEntity(name + " = " + value);
        }

        /// <summary>
        /// Writes an named data entity to current destination stream.
        /// </summary>
        /// <param name="name">The data entity name.</param>
        /// <param name="value">The data entity value.</param>
        public void Write(string name, double value)
        {
            WriteEntity(name + " = " + value);
        }

        /// <summary>
        /// Write the special char to current destination stream.
        /// </summary>
        /// <param name="type">The special char.</param>
        public void Write(SpecialChars type)
        {
            WriteEntity(Convert.ToChar(type).ToString());
        }

        /// <summary>
        /// Write start of an object or array using the first argument as its name.
        /// </summary>
        /// <param name="name">The name of the object or array.</param>
        /// <param name="type">The special char (StartObject or StartArray).</param>
        public void Write(string name, SpecialChars type)
        {
            WriteEntity(name + " = " + Convert.ToChar(type));
        }

        /// <summary>
        /// Get string with all special chars replaced (escaped) by their HTTP
        /// escape code (%NN). This functions is a noop if escaping is disabled.
        /// </summary>
        /// <param name="str">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        private string GetEscaped(string str)
        {
            if (escape)
            {
                str = str.Replace("(", String.Format("%{0:X}", Convert.ToUInt16('(')));
                str = str.Replace("[", String.Format("%{0:X}", Convert.ToUInt16('[')));
                str = str.Replace("]", String.Format("%{0:X}", Convert.ToUInt16(']')));
                str = str.Replace(")", String.Format("%{0:X}", Convert.ToUInt16(')')));
                str = str.Replace("=", String.Format("%{0:X}", Convert.ToUInt16('=')));
            }
            return str;
        }
    }
}
