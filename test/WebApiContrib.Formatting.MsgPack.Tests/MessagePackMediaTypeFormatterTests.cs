using System;
using System.Net.Http;
using NUnit.Framework;
using Should;

namespace WebApiContrib.Formatting.MsgPack.Tests
{
    public class Url
    {
        public int UrlId { get; set; }
        public string Address { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
    }

    public static class MessagePackMediaTypeFormatterTests
    {
        static HttpContent content;
        static MessagePackMediaTypeFormatter formatter;
        static Url data;

        [TestFixtureSetUp]
        public static void SetUp()
        {
            data = new Url
            {
                UrlId = 1,
                Address = "http://webapicontrib.github.com/",
                Title = "Web API Contrib",
                CreatedAt = DateTime.Now,
                CreatedBy = "Me",
            };

            formatter = new MessagePackMediaTypeFormatter();

            content = new ObjectContent<Url>(data, formatter);
        }

        [TestFixtureTearDown]
        public static void TearDown()
        {
            content.Dispose();
            content = null;
        }

        [Test]
        public static void Test_Should_Have_Headers()
        {
            content.Headers.ShouldNotBeNull();
            content.Headers.ShouldNotBeEmpty();
        }

        [Test]
        public static void Test_Content_Has_Correct_MediaType()
        {
            content.Headers.ContentType.ShouldNotBeNull();
            content.Headers.ContentType.MediaType.ShouldBeSameAs("application/x-msgpack");
        }

        [Test]
        public static async void Test_Content_Should_Be_Readable_As_Url()
        {
            var url = await content.ReadAsAsync<Url>(new[] { formatter });
            url.ShouldBeSameAs(data);
        }
    }
}
