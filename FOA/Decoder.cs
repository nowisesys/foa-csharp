using System;
using System.Text;
using System.IO;

namespace FOA
{
    /// <summary>
    /// This class represent an entity decoded from the FOA input stream.
    /// </summary>
    public struct Entity
    {
        public string Name;
        public string Data;
        public EntityType Type;
        public int Line;

        /// <summary>
        /// The type of entity.
        /// </summary>
        public enum EntityType
        {
            StartObject,
            StartArray,
            EndObject,
            EndArray,
            DataName
        }

        /// <summary>
        /// Constructs the entity object.
        /// </summary>
        /// <param name="name">The entity name (might be null)</param>
        /// <param name="data">The entity data (a special char or data)</param>
        /// <param name="type">The entity type.</param>
        /// <param name="line">Line number in the encoded stream where the entity where decoded from.</param>
        public Entity(string name, string data, EntityType type, int line)
        {
            this.Name = name;
            this.Data = data;
            this.Type = type;
            this.Line = line;
        }
    }

    /// <summary>
    /// This is a helper class used by the decoder class to group all buffer
    /// parsing related variables together.
    /// </summary>
    struct ParseData
    {
        public byte[] buffer; // Scan buffer.
        public int start;     // Scan start position.
        public int end;       // Scan end position.
        public int ppos;      // Current put position.
        public int line;      // Current line.
        public bool external; // Buffer is external.

        /// <summary>
        /// Reset the parse data. This method should be called prior to set
        /// a new stream or buffer to decode.
        /// </summary>
        /// <param name="external">Indicates whether the data to decode is external or not.</param>
        public void Reset(bool external)
        {
            if (external != this.external)
            {
                buffer = null;
            }
            this.external = external;

            line = start = end = ppos = 0;
        }

        /// <summary>
        /// Resize the buffer.
        /// </summary>
        /// <param name="size">The new size.</param>
        public void Resize(int size)
        {
            Array.Resize(ref buffer, size);
        }

        /// <summary>
        /// Move undecoded data to beginning of array. This method should be 
        /// called when no more entities where found in current buffer, but 
        /// before the buffer is filled again.
        /// </summary>
        public void MoveData()
        {
            int length = ppos - end;
            Array.Copy(buffer, end, buffer, 0, length);
            ppos = length; start = end = 0;
        }
    }
    
    /// <summary>
    /// This class implements the decoder of FOA.
    /// </summary>
    /// <remarks>
    /// It is possible to use an MemoryStream to decode an already pre-allocated buffer. 
    /// However, this would force an internal buffer to be allocated while only the external 
    /// is really required. As an solution a constructor taking an external buffer as an 
    /// argument is provided. In this case no memory is allocated internal by the decoder.
    /// </remarks>
    public class Decoder
    {

        private Stream stream;                       // Source stream.
        private ParseData data;                      // Parsing data.
        private MemoryStrategy strategy;             // Memory allocation strategy.
        private Encoding encoding = Encoding.UTF8;   // Input character encoding.
        private bool escape = true;  // Enable/disable escape sequences.

        /// <summary>
        /// Construct an decoder object. Call SetBuffer or SetStream to set
        /// the data source.
        /// </summary>
        public Decoder()
        {
        }

        /// <summary>
        /// Construct an decoder object decoding an external buffer.
        /// </summary>
        /// <param name="buffer">The external buffer to decode.</param>
        public Decoder(byte[] buffer)
        {
            SetBuffer(buffer);
        }

        /// <summary>
        /// Construct an decoder object decoding an external buffer.
        /// </summary>
        /// <param name="buffer">The external buffer to decode.</param>
        public Decoder(byte[] buffer, Encoding encoding)
        {
            SetBuffer(buffer);
            this.encoding = encoding;
        }

        /// <summary>
        /// Constructs an decoder object for decoding the string buffer.
        /// </summary>
        /// <param name="buffer">The string to decode.</param>
        /// <remarks>
        /// This is a convenience constructor that should only be used for short
        /// strings. Use the byte array constructor when in doubt.
        /// </remarks>
        public Decoder(string buffer)
        {
            SetBuffer(encoding.GetBytes(buffer));
        }

        /// <summary>
        /// Construct an decoder object.
        /// </summary>
        /// <param name="stream">The input source stream.</param>
        public Decoder(Stream stream)
        {
            this.strategy = new MemoryStrategy();
            this.stream = stream;
        }

        /// <summary>
        /// Construct an decoder object.
        /// </summary>
        /// <param name="stream">The input source stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public Decoder(Stream stream, Encoding encoding)
        {
            this.strategy = new MemoryStrategy();
            this.encoding = encoding;
            this.stream = stream;
        }

        /// <summary>
        /// Constructs an decoder object. 
        /// </summary>
        /// <param name="stream">The input source stream.</param>
        /// <param name="strategy">The memory allocation strategy to use.</param>
        public Decoder(Stream stream, MemoryStrategy strategy)
        {
            this.strategy = strategy;
            this.stream = stream;
        }

        /// <summary>
        /// Constructs an decoder object. 
        /// </summary>
        /// <param name="stream">The input source stream.</param>
        /// <param name="strategy">The memory allocation strategy to use.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public Decoder(Stream stream, MemoryStrategy strategy, Encoding encoding)
        {
            this.strategy = strategy;
            this.encoding = encoding;
            this.stream = stream;
        }

        /// <summary>
        /// Sets an new memory allocation strategy. Calling this method will resize
        /// the read buffer if the maximum buffer size has changed.
        /// </summary>
        /// <param name="strategy">The new memory allocation strategy.</param>
        public void SetStrategy(MemoryStrategy strategy)
        {
            if (strategy.MaxSize != MemoryStrategy.Unlimited &&
                strategy.MaxSize != this.strategy.MaxSize &&
                strategy.MaxSize < data.buffer.Length)
            {
                // Make sure we don't loose data on resize:
                if (strategy.MaxSize < data.buffer.Length - data.ppos)
                {
                    strategy.MaxSize = data.buffer.Length - data.ppos;
                }
                data.MoveData();
                data.Resize(strategy.MaxSize);
            }
            this.strategy = strategy;
        }

        /// <summary>
        /// Sets an new input stream to decode.
        /// </summary>
        /// <param name="stream">The input source stream.</param>
        public void SetStream(Stream stream)
        {
            data.Reset(false);
            this.stream = stream;
        }

        /// <summary>
        /// Get current source stream.
        /// </summary>
        /// <returns>The current used stream.</returns>
        public Stream GetStream()
        {
            return stream;
        }

        /// <summary>
        /// Sets a new buffer to decode.
        /// </summary>
        /// <param name="buffer">The buffer to decode.</param>
        public void SetBuffer(byte[] buffer)
        {
            data.Reset(true);
            data.buffer = buffer;
            data.ppos = buffer.Length;
        }

        /// <summary>
        /// Get the current buffer to decode.
        /// </summary>
        /// <returns>The byte array buffer.</returns>
        public byte[] GetBuffer()
        {
            return data.buffer;
        }

        /// <summary>
        /// Sets the character encoding.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        public void SetEncoding(Encoding encoding)
        {
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
        /// Read the next entity from the current stream or buffer. This function
        /// throws an DecoderException if the input data can't be decoded or if
        /// the maximum buffer size is exceeded.
        /// </summary>
        /// <param name="entity">Set to decoded entity.</param>
        /// <returns>False when no more entities exists.</returns>
        public bool Read(ref Entity entity)
        {
            if (data.external)
            {
                if (!FindNext())
                {
                    return false;
                }
            }
            else
            {
                if (!FindNext())
                {
                    // Move undecoded data to beginning of the buffer and then 
                    // attempt to fill the scan buffer.
                    if (data.buffer != null)
                    {
                        data.MoveData();
                    }
                    if (!FillBuffer())
                    {
                        return false;
                    }
                }
            }

            DecodeNext(ref entity);
            return true;
        }

        /// <summary>
        /// Decode next entity in the buffer. This function should only be called
        /// after a successful call to FindNext() to get the start and end of the
        /// entity to decode set.
        /// </summary>
        /// <param name="entity">Set to decoded entity.</param>
        private void DecodeNext(ref Entity entity)
        {
            entity.Line = data.line;

            string str = encoding.GetString(data.buffer, data.start, data.end - data.start).Trim();
            if (str.IndexOf('=') != -1)
            {
                entity.Name = str.Substring(0, str.IndexOf('=')).Trim();
                entity.Data = str.Substring(str.IndexOf('=') + 1).Trim();
            }
            else
            {
                entity.Name = null;
                entity.Data = str;
            }
            if (entity.Data.Length == 1)
            {
                switch (entity.Data[0])
                {
                    case '(':
                        entity.Type = Entity.EntityType.StartObject;
                        break;
                    case '[':
                        entity.Type = Entity.EntityType.StartArray;
                        break;
                    case ']':
                        entity.Type = Entity.EntityType.EndArray;
                        break;
                    case ')':
                        entity.Type = Entity.EntityType.EndObject;
                        break;
                }
            }
            else
            {
                entity.Type = Entity.EntityType.DataName;
                if (escape)
                {
                    entity.Data = GetEscaped(entity.Data);
                }
            }
        }

        /// <summary>
        /// Tries to find the start and end position of next entity to decode.
        /// </summary>
        /// <returns>True if an entity to decode was found.</returns>
        private bool FindNext()
        {
            if (data.buffer == null)
            {
                return false;
            }

            ParseData curr = data;
            while (data.end < data.ppos && data.buffer[data.end] == '\n')
            {
                data.end++;
            }
            if (data.end >= data.ppos)
            {
                data = curr;
                return false;
            }

            data.start = data.end;
            while (data.end < data.ppos && data.buffer[data.end] != '\n')
            {
                data.end++;
            }
            if (data.end >= data.ppos)
            {
                data = curr;
                return false;
            }

            data.line++;
            return true;
        }

        /// <summary>
        /// Try to fill the scan buffer (data) possibly extending the buffer. This
        /// function throws an DecoderException if maximum buffer size is exceeded.
        /// 
        /// Note that this function calls FindNext() internal to check when its 
        /// done. If it returns true, then 
        /// </summary>
        /// <returns></returns>
        private bool FillBuffer()
        {
            if (data.buffer == null)
            {
                data.buffer = new byte[strategy.InitSize];
            }
            while (true)
            {
                int want = data.buffer.Length - data.ppos;
                int read = stream.Read(data.buffer, data.ppos, want);

                if (read == 0)
                {
                    return false;
                }
                data.ppos += read;

                if (FindNext())
                {
                    return true;
                }

                if (read == want)
                {
                    int size = data.buffer.Length + strategy.StepSize;
                    if (size > strategy.MaxSize && strategy.MaxSize != MemoryStrategy.Unlimited)
                    {
                        throw new DecoderException("Maximum decoder buffer size exceeded.");
                    }
                    data.Resize(size);
                }
            }
            return false;
        }

        /// <summary>
        /// Get string with all HTTP escape codes (%NN) replaced by their special
        /// chars equivalents.
        /// </summary>
        /// <param name="str">The string to unescape.</param>
        /// <returns>The unescaped string.</returns>
        private string GetEscaped(string str)
        {
            str = str.Replace(String.Format("%{0:X}", Convert.ToUInt16('(')), "(");
            str = str.Replace(String.Format("%{0:X}", Convert.ToUInt16('[')), "[");
            str = str.Replace(String.Format("%{0:X}", Convert.ToUInt16(']')), "]");
            str = str.Replace(String.Format("%{0:X}", Convert.ToUInt16(')')), ")");
            str = str.Replace(String.Format("%{0:X}", Convert.ToUInt16('=')), "=");

            return str;
        }
    }
}
