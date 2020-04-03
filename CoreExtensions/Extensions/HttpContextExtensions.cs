using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class HttpContextExtensions
    {
        public static string ExtractBody(this HttpRequest request)
            => InternalExtractBody(request);

        public static string ExtractBody(this HttpResponse response)
            => InternalExtractBody(response);

        private static string InternalExtractBody(dynamic request)
        {
            var bodyAsText = new StreamReader(request.Body).ReadToEnd();
            var bodyData = Encoding.UTF8.GetBytes(bodyAsText);
            request.Body = new MemoryStream(bodyData);
            return bodyAsText;
        }

        public static async Task<string> ExtractRequestBody(this HttpRequest request)
        {
            request.EnableRewind();
            var body = request.Body;
            RewindBody(body);

            var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            RewindBody(body);
            request.Body = body;
            return bodyAsText;
        }

        private static void RewindBody(Stream body)
        {
            if (body.CanSeek)
                body.Seek(0, SeekOrigin.Begin);
        }

    }
}
