using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using MsgPack.Serialization;

namespace WebApiContrib.Formatting.MsgPack
{
    /// <summary>
    /// A media type formatter using the Message Pack specification.
    /// </summary>
    /// <remarks>Converted from Filip W.'s MessagePackMediaTypeFormatter.</remarks>
    /// <see href="http://msgpack.org/"/>
    /// <see href="http://www.strathweb.com/2012/09/boost-up-your-asp-net-web-api-with-messagepack/"/>
    public class MessagePackMediaTypeFormatter : BufferedMediaTypeFormatter
    {
        private const string mediaType = "application/x-msgpack";

        private static bool IsAllowedType(Type t)
        {
            if (t != null && !t.IsAbstract && !t.IsInterface && !t.IsNotPublic)
                return true;

            if (typeof(IEnumerable).IsAssignableFrom(t))
                return true;

            return false;
        }

        public MessagePackMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
        }

        public override bool CanReadType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type is null");

            return IsAllowedType(type);
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type is null");

            return IsAllowedType(type);
        }

        public override object ReadFromStream(Type type, System.IO.Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            if (content.Headers != null && content.Headers.ContentLength == 0)
                return null;

            object result;
            try
            {
                var packer = SerializationContext.Default.GetSerializer(type);
                result = packer.Unpack(readStream);
            }
            catch (Exception ex)
            {
                if (formatterLogger == null)
                    throw;

                formatterLogger.LogError("", ex.Message);
                result = GetDefaultValueForType(type);
            }

            return result;
        }

        public override void WriteToStream(Type type, object value, System.IO.Stream writeStream, System.Net.Http.HttpContent content)
        {
            if (type == null)
                throw new ArgumentNullException("type is null");

            if (writeStream == null)
                throw new ArgumentNullException("writeStream is null");

            if (typeof(IEnumerable).IsAssignableFrom(type))
                value = (value as IEnumerable<object>).ToList();

            var packer = SerializationContext.Default.GetSerializer(type);
            packer.Pack(writeStream, value);
        }
    }
}
