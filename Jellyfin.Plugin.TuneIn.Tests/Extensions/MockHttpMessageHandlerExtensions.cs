using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.TuneIn.Tests.Extensions
{
    public static class MockHttpMessageHandlerExtensions
    {
        public static MockedRequest WhenGet(this MockHttpMessageHandler mockHttpMessageHandler, string url)
        {
            return mockHttpMessageHandler.When(HttpMethod.Get, url);
        }

        public static MockedRequest WhenHead(this MockHttpMessageHandler mockHttpMessageHandler, string url)
        {
            return mockHttpMessageHandler.When(HttpMethod.Head, url);
        }

        public static void RespondOk(this MockedRequest mockedRequest, string content, Dictionary<string, string>? headers = default, string? contentType = default)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content),
            };
            if (contentType != default)
            {
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }
            if (headers != default)
            {
                foreach(var header in headers)
                {
                    response.Content.Headers.Add(header.Key, header.Value);
                }
            }

            mockedRequest.Respond(() => Task.FromResult(response));
        }

        public static void RespondOk(this MockedRequest mockedRequest, Stream content, Dictionary<string, string>? headers = default, string? contentType = default)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(content),
            };
            if (contentType != default)
            {
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }
            if (headers != default)
            {
                foreach (var header in headers)
                {
                    response.Content.Headers.Add(header.Key, header.Value);
                }
            }

            mockedRequest.Respond(() => Task.FromResult(response));
        }
        public static void RespondOk(this MockedRequest mockedRequest, Dictionary<string, string>? headers = default, string? contentType = default)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ReadOnlyMemoryContent(null) };

            if (contentType != default)
            {
                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
            }

            if (headers != default)
            {
                foreach (var header in headers)
                {
                    response.Content.Headers.Add(header.Key, header.Value);
                }
            }

            mockedRequest.Respond(() => Task.FromResult(response));
        }

        public static void RespondOkWithEmbededResource<TAssembly>(this MockedRequest mockedRequest, string embededReource)
        {
            mockedRequest
                .RespondOk(typeof(TAssembly).Assembly.GetManifestResourceStream($"{typeof(TAssembly).Namespace}.{embededReource}")!);
        }
    }
}
