using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    /// <summary>
    /// クライアントを表すクラス
    /// </summary>
    class Client
    {
        // 接続先サーバーのエンドポイント
        private static readonly IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback, 22222);
        private static readonly byte[] buffer = new byte[2000];
        private const byte version = 0;
        private const byte login = 0;
        private const int usernameByteSize = 32;
        private const int passwordByteSize = 32;

        /// <summary>
        /// クライアントの動作
        /// </summary>
        static void Main()
        {
            // TCP/IPでの通信を行うソケットの作成
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // Ctrl+Cが押された場合はソケットを閉じる
                Console.CancelKeyPress += (sender, args) => socket.Close();

                socket.Connect(serverEndPoint);
                Console.WriteLine("connected to {0}", socket.RemoteEndPoint);
                Console.Write("username:");
                string inputUsername = Console.ReadLine();
                inputUsername = inputUsername.Replace(Environment.NewLine, "");
                Console.Write("password:");
                string inputPassword = Console.ReadLine();
                inputPassword = inputPassword.Replace(Environment.NewLine, "");
                // 非同期での受信を開始する
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, socket);
                byte[] header = { version, login };
                byte[] username = Encoding.Default.GetBytes(PaddingInBytes(inputUsername,usernameByteSize));
                byte[] password = Encoding.Default.GetBytes(PaddingInBytes(inputPassword, passwordByteSize));
                byte[] body = username.Concat(password).ToArray();
                byte[] loginMessage = header.Concat(body).ToArray();
                socket.Send(loginMessage);
                 
                for (; ; )
                {
                    var input = Console.ReadLine();

                    // Ctrl+Z(UNIXではCtrl+D)が押された場合
                    if (input == null)
                    {
                        socket.Close();
                        break;
                    }

                    socket.Send(Encoding.Default.GetBytes(input + Environment.NewLine));
                }
            }
        }

        // 非同期受信のコールバックメソッド
        private static void ReceiveCallback(IAsyncResult ar)
        {
            var socket = ar.AsyncState as Socket;

            // 受信を待機する
            var len = socket.EndReceive(ar);

            // 受信した内容を表示する
            Console.Write("> {0}", Encoding.Default.GetString(buffer, 0, len));

            // 再度非同期での受信を開始する
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, socket);
        }

        private static string PaddingInBytes(string value, int byteCount)
        {
            return value.PadLeft(byteCount,'\0');
        }


    }
}
