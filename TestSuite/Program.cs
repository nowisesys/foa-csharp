using System;
using System.Text;
using System.IO;

namespace TestSuite
{
    class Program
    {
        private static void PrintEntity(ref FOA.Entity entity)
        {
            if (entity.Name != null)
            {
                Console.WriteLine("{0,3}: {1} = {2}\t({3})", entity.Line, entity.Name, entity.Data, entity.Type);
            }
            else
            {
                Console.WriteLine("{0,3}: {1}\t({2})", entity.Line, entity.Data, entity.Type);
            }
        }

        /// <summary>
        /// Encode data writing the result to an memory stream.
        /// </summary>
        private static void EncodeMemory()
        {
            FOA.Encoder encoder = new FOA.Encoder(new MemoryStream());

            // 
            // Write an object.
            // 
            encoder.Write("person", FOA.SpecialChars.StartObject);
            encoder.Write("name", "Adam");
            encoder.Write("age", 35);
            encoder.Write("ratio", 8.37);
            encoder.Write(FOA.SpecialChars.EndObject);

            // 
            // Get memory stream and convert its byte array to an string object.
            // 
            MemoryStream stream = (MemoryStream)encoder.GetStream();
            Console.WriteLine(Encoding.Unicode.GetString(stream.GetBuffer()));
        }

        /// <summary>
        /// Encode data writing the result to a text file.
        /// </summary>
        private static void EncodeFile()
        {
            using (FileStream writer = new FileStream("output.txt", FileMode.Create))
            {
                FOA.Encoder encoder = new FOA.Encoder(writer);

                // 
                // Write an object.
                // 
                encoder.Write("person", FOA.SpecialChars.StartObject);
                encoder.Write("name", "Adam");
                encoder.Write("age", 35);
                encoder.Write("ratio", 8.37);
                encoder.Write(FOA.SpecialChars.EndObject);
            }
        }

        /// <summary>
        /// Encode the data without using a backing store, that is, the encoded
        /// data is not appended to a stream. Instead only encoded data from the
        /// last Write() is stored in the buffer.
        /// </summary>
        private static void EncodeNoStore()
        {
            FOA.Encoder encoder = new FOA.Encoder();

            encoder.Write("person", FOA.SpecialChars.StartObject);
            Console.Write(encoder.GetString());
            encoder.Write("name", "Adam");
            Console.Write(encoder.GetString());
            encoder.Write("age", 35);
            Console.Write(encoder.GetString());
            encoder.Write("ratio", 8.37);
            Console.Write(encoder.GetString());
            encoder.Write(FOA.SpecialChars.EndObject);
            Console.Write(encoder.GetString());
        }

        /// <summary>
        /// Encode special chars.
        /// </summary>
        private static void EncodeEscaped()
        {
            FOA.Encoder encoder = new FOA.Encoder();

            Console.WriteLine("Escape enabled:");
            encoder.Write("a(b[c]d)e=f");
            Console.Write(encoder.GetString());

            Console.WriteLine("Escape disabled:");
            encoder.SetOption(FOA.Option.EnableEscape, false);
            encoder.Write("a(b[c]d)e=f");
            Console.Write(encoder.GetString());
        }

        /// <summary>
        /// Decode special chars.
        /// </summary>
        private static void DecodeEscaped()
        {
            FOA.Entity entity = new FOA.Entity();
            FOA.Decoder decoder = new FOA.Decoder();

            Console.WriteLine("Escape enabled:");
            decoder.SetBuffer(Encoding.UTF8.GetBytes("a%28b%5Bc%5Dd%29e%3Df\n"));
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }

            decoder.SetBuffer(Encoding.UTF8.GetBytes("name = a%28b%5Bc%5Dd%29e%3Df\n"));
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }

            Console.WriteLine("Escape disabled:");
            decoder.SetOption(FOA.Option.EnableEscape, false);

            decoder.SetBuffer(Encoding.UTF8.GetBytes("name = a%28b%5Bc%5Dd%29e%3Df\n"));
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }
        }

        /// <summary>
        /// Decode a FOA object from a byte array.
        /// </summary>
        private static void DecodeBufferObject()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("obj = (\nname = adam\nage = 24\n)\n");
            FOA.Decoder decoder = new FOA.Decoder(buffer);
            FOA.Entity entity = new FOA.Entity();
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }
        }

        /// <summary>
        /// Decode a FOA object from a memory stream.
        /// </summary>
        private static void DecodeMemoryStream()
        {
            byte[] buffer = Encoding.UTF8.GetBytes("obj = (\nname = adam\nage = 24\n)\n");
            MemoryStream stream = new MemoryStream(buffer);
            FOA.Decoder decoder = new FOA.Decoder(stream);
            FOA.Entity entity = new FOA.Entity();
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }
        }

        /// <summary>
        /// Decode FOA entities from a file stream.
        /// </summary>
        private static void DecodeFileStream()
        {
            using (FileStream stream = new FileStream("..\\..\\Data.txt", FileMode.Open))
            {
                FOA.Decoder decoder = new FOA.Decoder(stream);
                FOA.Entity entity = new FOA.Entity();
                while (decoder.Read(ref entity))
                {
                    PrintEntity(ref entity);
                }
            }
        }

        /// <summary>
        /// Decode a simple, anonymous object.
        /// </summary>
        private static void DecodeSimpleObject()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(String.Format("(\nadam\n24\n)\n"));
            FOA.Decoder decoder = new FOA.Decoder(buffer);
            FOA.Entity entity = new FOA.Entity();
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }
        }

        /// <summary>
        /// Decode and array of objects.
        /// </summary>
        private static void DecodeObjectArray()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(String.Format("arr = [\nobj1 = (\nname = adam\nage = 24\n)\nobj2 = (\nname = adam\nage = 24\n)\n]\n"));
            FOA.Decoder decoder = new FOA.Decoder(buffer);
            FOA.Entity entity = new FOA.Entity();
            while (decoder.Read(ref entity))
            {
                PrintEntity(ref entity);
            }
        }

        /// <summary>
        /// Decode a *lot* of objects. This code might throw an OutOfMemoryException (catched).
        /// </summary>
        private static void DecodeMultiObject()
        {
            long num = 20000000, i;
            string obj = "(adam\n34\n)\n";

            try
            {
                Console.Write("Creating object array... ");
                StringBuilder sb = new StringBuilder("[\n");
                for (i = 0; i < num; ++i)
                {
                    sb.Append(obj);
                }
                sb.Append("]\n");
                Console.WriteLine("done");

                Console.Write("Contructing decoder object... ");
                FOA.Decoder decoder = new FOA.Decoder(sb.ToString());
                Console.WriteLine("done");

                FOA.Entity entity = new FOA.Entity();
                Console.Write("Decoding {0} objects... ", num);
                while (decoder.Read(ref entity))
                {
                }
                Console.WriteLine("done");
            }
            catch (OutOfMemoryException exception)
            {
                Console.Error.WriteLine(exception.Message);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: TestSuite <test>");
                Console.WriteLine("Where <test> is one of:");
                Console.WriteLine("  EncodeMemory         - Write encoded data to memory stream");
                Console.WriteLine("  EncodeFile           - Write encoded data to a file stream");
                Console.WriteLine("  EncodeNoStore        - Encode data without using a backing store stream");
                Console.WriteLine("  EncodeEscaped        - Encode data with escaped special chars");
                Console.WriteLine("  DecodeEscaped        - Decode data with escaped special chars");
                Console.WriteLine("  DecodeBufferObject   - Decode an FOA object in string buffer");
                Console.WriteLine("  DecodeMemoryStream   - Decode an FOA object thru a memory stream");
                Console.WriteLine("  DecodeFileStream     - Decode an FOA object thru a file stream");
                Console.WriteLine("  DecodeSimpleObject   - Decode an simple FOA object (anonymous)");
                Console.WriteLine("  DecodeObjectArray    - Decode an array of objects");
                Console.WriteLine("  DecodeMultiObject    - Decode an huge number of objects");
                Environment.Exit(1);
            }

            switch(args[0]) {
                case "EncodeMemory":
                    EncodeMemory();
                    break;
                case "EncodeFile":
                    EncodeFile();
                    break;
                case "EncodeNoStore":
                    EncodeNoStore();
                    break;
                case "EncodeEscaped":
                    EncodeEscaped();
                    break;
                case "DecodeEscaped":
                    DecodeEscaped();
                    break;
                case "DecodeBufferObject":
                    DecodeBufferObject();
                    break;
                case "DecodeMemoryStream":
                    DecodeMemoryStream();
                    break;
                case "DecodeFileStream":
                    DecodeFileStream();
                    break;
                case "DecodeSimpleObject":
                    DecodeSimpleObject();
                    break;
                case "DecodeObjectArray":
                    DecodeObjectArray();
                    break;
                case "DecodeMultiObject":
                    DecodeMultiObject();
                    break;
            }
        }
    }
}
