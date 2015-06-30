using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace EchoServer
{
    internal class Session
    {
        private readonly int id;
        private Socket client;

        public Session(int id, Socket socket)
        {
            this.id = id;
            this.client = socket;
        }

        public void Start()
        {
            // スレッドを作成し、クライアントを処理する
            var t = new Thread(SessionProc);

            t.Start();
        }

        private void SessionProc()
        {
            Console.WriteLine("client ID #{0} : session started", id);

            try
            {
                byte[] buffer = new byte[2000];

                var loginDataLength = client.Receive(buffer);

                if (loginDataLength == 66)
                {
                    if (buffer[1] == 0)
                    {
                        string closeMessage = "loggedin";
                        byte[] buff = System.Text.Encoding.ASCII.GetBytes(closeMessage);
                        client.Send(buff);
                    }
                }
                else
                {
                    string closeMessage = "Bye.";
                    byte[] buff = System.Text.Encoding.ASCII.GetBytes(closeMessage);
                    client.Send(buff);
                    return;
                }

                for (; ; )
                {
                    // クライアントから送信された内容を受信する
                    var len = client.Receive(buffer);

                    if (0 < len)
                    {
                        // 受信した内容を表示する
                        Console.Write("#{0}> {1}", id, Encoding.Default.GetString(buffer, 0, len));

                        // 受信した内容した内容をそのままクライアントに送信する
                        client.Send(buffer, len, SocketFlags.None);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode != SocketError.ConnectionReset)
                    // 切断された以外の場合では例外を再スローする
                    throw;
            }
            finally
            {
                client.Close();
                Console.WriteLine("#{0} session closed", id);
            }
        }
    }
}
