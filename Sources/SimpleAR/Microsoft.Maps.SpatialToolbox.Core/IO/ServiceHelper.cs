using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// A helpful tool for making asynchronous service calls as Tasks.
    /// </summary>
    public class ServiceHelper
    {
        /// <summary>
        /// Downloads JSON data from a URL and casts it to the specified Type.
        /// </summary>
        /// <typeparam name="T">The object type to cast the data to.</typeparam>
        /// <param name="url">URL that points to data to download.</param>
        /// <returns>A data object of the specified type.</returns>
        public static Task<T> GetJsonAsync<T>(Uri url) where T : new()
        {
            return Task.Run<T>(async () =>
            {
                try
                {
                    using (var stream = await GetStreamAsync(url))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            var t = new T();
                            return Newtonsoft.Json.JsonConvert.DeserializeAnonymousType<T>(reader.ReadToEnd(), t, null);
                        }
                    }
                }
                catch { }

                return default(T);
            });
        }

        /// <summary>
        /// Downloads data as a string from a URL.
        /// </summary>
        /// <param name="url">URL that points to data to download.</param>
        /// <returns>A string with the data.</returns>
        public static Task<string> GetStringAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<string>();

            var request = HttpWebRequest.Create(url);
            request.BeginGetResponse((a) =>
            {
                try
                {
                    var r = (HttpWebRequest)a.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)r.EndGetResponse(a);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        tcs.SetResult(reader.ReadToEnd());
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, request);

            return tcs.Task;
        }

        /// <summary>
        /// Downloads data as a stream from a URL.
        /// </summary>
        /// <param name="url">URL that points to data to download.</param>
        /// <returns>A stream with the data.</returns>
        public static Task<Stream> GetStreamAsync(Uri url)
        {
            var tcs = new TaskCompletionSource<Stream>();

            var request = HttpWebRequest.Create(url);
            request.BeginGetResponse((a) =>
            {
                try
                {
                    var r = (HttpWebRequest)a.AsyncState;
                    HttpWebResponse response = (HttpWebResponse)r.EndGetResponse(a);
                    tcs.SetResult(response.GetResponseStream());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, request);

            return tcs.Task;
        }

        /// <summary>
        /// Makes a post request using string data. 
        /// </summary>
        /// <param name="url">URL to post data to.</param>
        /// <param name="data">String representation of data to be posted to service.</param>
        /// <returns>Response stream.</returns>
        public static Task<Stream> PostStringAsync(Uri url, string data)
        {
            var tcs = new TaskCompletionSource<Stream>();

            //Include the data to geocode in the HTTP request
            var request = HttpWebRequest.Create(url);

            request.Method = "POST";
            request.ContentType = "text/plain;charset=utf-8";

            request.BeginGetRequestStream((a) =>
            {
                try
                {
                    var r = (HttpWebRequest)a.AsyncState;

                    using (var requestStream = r.EndGetRequestStream(a))
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, request);

            request.BeginGetResponse((a) =>
            {
                try
                {
                    var r = (HttpWebRequest)a.AsyncState;

                    using (var response = (HttpWebResponse)r.EndGetResponse(a))
                    {
                        tcs.SetResult(response.GetResponseStream());
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, request);

            return tcs.Task;
        }
    }
}
