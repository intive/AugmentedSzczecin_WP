using Microsoft.Phone.Maps.Controls;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Microsoft.Maps.SpatialToolbox.Bing
{    
    /// <summary>
    /// A class for creating a local tile source.
    /// Based on: http://kodira.de/2013/01/offline-maps-with-windows-phone-8/
    /// </summary>
    public class LocalTileDataSource : TileSource
    {
        #region Private Properties

        private StreamSocketListener server;
        private string portNumber = "8081";

        #endregion

        #region Constructor

        /// <summary>
        /// A class for creating a local tile source.
        /// </summary>
        public LocalTileDataSource()
        {
            CreateLocalServer();
        }

        /// <summary>
        /// A class for creating a local tile source.
        /// </summary>
        /// <param name="uriFormat">
        /// The url format for accessing the local tiles. Supported URL parameters: {quadkey}, {x}, {y}, {zoomlevel}
        /// <para>
        /// Example: OfflineTiles/{quadkey}.png
        /// </para>        
        /// </param>
        public LocalTileDataSource(string uriFormat)
            : base(uriFormat)
        {
            CreateLocalServer();
        }

        ~LocalTileDataSource()
        {
            if (server != null)
            {
                server.Dispose();
                server = null;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The port number to be used by the local server for the tile layer.
        /// </summary>
        public string PortNumber
        {
            get
            {
                return portNumber;
            }
            set
            {
                portNumber = value;
                CreateLocalServer();
            }
        }

        #endregion

        #region Public Methods

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            var quadkey = TileMath.TileXYToQuadKey(x, y, zoomLevel);
            var uri = new Uri("http://localhost:" + portNumber + "/" + this.UriFormat.Replace("{quadkey}", quadkey).Replace("{x}", x.ToString().Replace("{y}", y.ToString()).Replace("{zoomlevel}", zoomLevel.ToString())));
            return uri;
        }

        #endregion

        #region Private Properties

        private async void CreateLocalServer()
        {
            if (server != null)
            {
                server.Dispose();
                server = null;
            }

            server = new StreamSocketListener();
            server.ConnectionReceived += server_ConnectionReceived;
            await server.BindServiceNameAsync("8081");
        }

        private async void server_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                var reader = new DataReader(args.Socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                uint numStrBytes = await reader.LoadAsync(512);
                string request = reader.ReadString(numStrBytes);

                using (IOutputStream output = args.Socket.OutputStream)
                {
                    string requestMethod = request.Split('\n')[0];
                    string[] requestParts = requestMethod.Split(' ');

                    if (requestParts[0] == "GET")
                        await SendResponse(requestParts[1], output);
                }
            }
            catch (Exception ex)
            {
                var t = ex;
            }
        }

        private async Task SendResponse(string path, IOutputStream os)
        {
            if (path.StartsWith("/"))
            {
                path = path.Substring(1, path.Length - 1);
            }

            var resource = Application.GetResourceStream(new Uri(path, UriKind.Relative));

            if (resource != null && resource.Stream != null)
            {
                using (var resp = os.AsStreamForWrite())
                {
                    using (var fs = resource.Stream)
                    {
                        string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                        "Content-Length: {0}\r\n" +
                                        "Content-Type:  image/png\r\n" +
                                        "Connection: close\r\n" +
                                        "\r\n",
                                        fs.Length);
                        byte[] headerArray = Encoding.UTF8.GetBytes(header);
                        await resp.WriteAsync(headerArray, 0, headerArray.Length);
                        await fs.CopyToAsync(resp);
                    }
                }
            }
            else
            {
                using (var resp = os.AsStreamForWrite())
                {
                    string body = "<HTML><HEAD></HEAD><BODY><P style=\"font-size:initial; color:#0000FF\">If you see this, the StreamSocketListener is working.</P></BODY></HTML>";
                    string header = String.Format("HTTP/1.1 200 OK\r\nContent-Length: {0}\r\nContent-Type: text/html\r\nConnection: close\r\n\r\n{1}\r\n", body.Length, body);
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                    await resp.FlushAsync();
                }
            }
        }

        #endregion
    }
}
