using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GopherLib.Models;

namespace GopherLib
{
    public class GopherClient
    {
        public class InvalidUriException : Exception
        {
            public InvalidUriException() : base() { }
            public InvalidUriException(string message) : base(message) { }
        }

        public delegate byte[] ReadGopherEntityDelegate(GopherEntity gopherEntity);

        public static async Task<byte[]> GetGopherEntity(GopherEntity gopherEntity)
        {
            byte[] result = null;

            if (gopherEntity.IsGopherScheme)
            {
                // Read the entity from the net
                ReadGopherEntityDelegate del = ReadGopherEntity;
                return await Task.Run(() => del.Invoke(gopherEntity));
            }
            else
            {
                // Read the entity from a file
                var text = await System.IO.File.ReadAllTextAsync(gopherEntity.DisplayText).ConfigureAwait(false);
                result = Encoding.UTF8.GetBytes(text);
            }
            return result;
        }
        private static byte[] ReadGopherEntity(GopherEntity gopherEntity)
        {
            var results = Array.Empty<byte>();

            var socket = ConnectSocket(gopherEntity.Host, gopherEntity.Port);
            if (socket != null)
            {
                // Send selector to the server, and search term if there is one
                var payload = gopherEntity.Selector;
                if (!string.IsNullOrWhiteSpace(gopherEntity.SearchTerms)) payload += $"\t{gopherEntity.SearchTerms}";
                payload += "\r\n";
                socket.Send(Encoding.ASCII.GetBytes(payload));

                var buffer = new byte[1024];
                int bytesRead;
                do
                {
                    bytesRead = socket.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        // Grow the results buffer and append the new bytes to it
                        var temp = new byte[results.Length + bytesRead];
                        Buffer.BlockCopy(results, 0, temp, 0, results.Length);
                        Buffer.BlockCopy(buffer, 0, temp, results.Length, bytesRead);
                        results = temp;
                    }
                }
                while (bytesRead > 0);
            }
            else throw new InvalidUriException();
            return results;
        }

        private static Socket ConnectSocket(string server, int port)
        {
            Socket socket = null;
            IPHostEntry hostEntry = null;

            // Get host related information.
            hostEntry = Dns.GetHostEntry(server);
            // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
            // an exception that occurs when the host IP Address is not compatible with the address family
            // (typical in the IPv6 case).
            foreach (IPAddress address in hostEntry.AddressList)
            {
                IPEndPoint ipEndPoint = new IPEndPoint(address, port);
                Socket tempSocket =
                    new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                tempSocket.Connect(ipEndPoint);

                if (tempSocket.Connected)
                {
                    socket = tempSocket;
                    break;
                }
                else
                {
                    tempSocket.Close();
                }
            }
            return socket;
        }
    }
}
