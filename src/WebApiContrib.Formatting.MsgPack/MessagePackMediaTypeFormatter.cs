using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MsgPack.Serialization;

namespace WebApiContrib.Formatting.MsgPack
{
    /// <summary>
    /// A media type formatter using the Message Pack specification.
    /// </summary>
    /// <remarks>Converted from Filip W.'s MessagePackMediaTypeFormatter.</remarks>
    /// <see href="http://msgpack.org/"/>
    /// <see href="http://www.strathweb.com/2012/09/boost-up-your-asp-net-web-api-with-messagepack/"/>
    public class MessagePackMediaTypeFormatter : MediaTypeFormatter
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
                throw new ArgumentNullException("type");

            return IsAllowedType(type);
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return IsAllowedType(type);
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
        {
            if (content.Headers != null && content.Headers.ContentLength == 0)
                return null;

            object result;
            try
            {
                var packer = SerializationContext.Default.GetSerializer(type);
                result = packer.Unpack(stream);
            }
            catch (Exception ex)
            {
                if (formatterLogger == null)
                    throw;

                formatterLogger.LogError("", ex.Message);
                result = GetDefaultValueForType(type);
            }

            return Task.FromResult(result);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContent content, TransportContext transportContext)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (stream == null)
                throw new ArgumentNullException("stream");

            if (typeof(IEnumerable).IsAssignableFrom(type))
                value = (value as IEnumerable<object>).ToList();

            var tcs = new TaskCompletionSource<object>();

            try
            {
                var packer = SerializationContext.Default.GetSerializer(type);
                packer.Pack(stream, value);

                tcs.SetResult(null);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }

            return tcs.Task;
        }
    }
}
