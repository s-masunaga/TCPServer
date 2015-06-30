using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace EchoServer
{
    /// <summary>
    /// エコーサーバを表すクラス
    /// </summary>
    class EchoServer
    {
        private static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 22222);

        /// <summary>
        /// 実際の動き
        /// </summary>
        static void Main()
        {
            using (var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // TIME_WAIT状態のソケットを再利用する
                server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // ソケットをアドレスにバインドする
                server.Bind(endPoint);
                
                // 接続の待機を開始する
                server.Listen(10);

                Console.WriteLine("server started ({0})", server.LocalEndPoint);

                var connectionCount = 0;

                for (; ; )
                {
                    // クライアントからの接続要求を待機する
                    var client = server.Accept();

                    connectionCount++;

                    Console.WriteLine("client accepted (#{0}, {1})", connectionCount, client.RemoteEndPoint);

                    // 新しくスレッドを作成してクライアントを処理する
                    StartSessionInNewThread(connectionCount, client);

                }
            }
        }

        private static void StartSessionInNewThread(int clientId, Socket client)
        {
            var session = new Session(clientId, client);

            session.Start();
        }
    }
}
