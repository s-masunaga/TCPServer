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
        private const int HeaderLength = 2;
        private const int UsernameLength = 32;
        private const int PasswordLength = 32;
        private readonly int id;
        private Socket client;
        private string username;

        List<UserInfo> userInfos = new List<UserInfo> { new UserInfo() { UserName = "masunaga", Password = "secret" }, new UserInfo() { UserName = "yamada", Password = "secret" } };

        public Session(int id, Socket socket)
        {
            this.id = id;
            this.client = socket;
        }

        public void Start()
        {
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

                if (loginDataLength == (HeaderLength + UsernameLength + PasswordLength))
                {
                    if (buffer[1] == 0x00)
                    {
                        byte[] usernameBuff = new byte[UsernameLength];
                        Array.Copy(buffer, HeaderLength, usernameBuff, 0, UsernameLength);
                        byte[] passwordBuff = new byte[PasswordLength];
                        Array.Copy(buffer, HeaderLength + UsernameLength, passwordBuff, 0, PasswordLength);
                        var inputUsername = Encoding.Default.GetString(usernameBuff);
                        inputUsername = inputUsername.Replace("\0", "");
                        var inputPassword = Encoding.Default.GetString(passwordBuff);
                        inputPassword = inputPassword.Replace("\0", "");

                        var selectUsernames = from x in userInfos
                                              where x.UserName == inputUsername && x.Password == inputPassword
                                              select x.UserName;

                        if (selectUsernames.Count() == 1)
                        {
                            string loginMessage = "loggedin\n";
                            byte[] buff = System.Text.Encoding.Default.GetBytes(loginMessage);
                            client.Send(buff);
                            this.username = selectUsernames.First();
                        }
                        else
                        {
                            SendCloseMessage();
                            return;
                        }
                    }
                    else
                    {
                        SendCloseMessage();
                        return;
                    }

                }
                else
                {
                    SendCloseMessage();
                    return;
                }

                for (; ; )
                {
                    // クライアントから送信された内容を受信する
                    var len = client.Receive(buffer);

                    if (0 < len)
                    {
                        // 受信した内容を表示する
                        Console.Write("#{0}> {1}", this.username, Encoding.Default.GetString(buffer, 0, len));

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

        private void SendCloseMessage()
        {
            string closeMessage = "Bye.\n";
            byte[] buff = System.Text.Encoding.ASCII.GetBytes(closeMessage);
            client.Send(buff);
        }


    }

    internal class UserInfo
    {
        internal string UserName { get; set; }
        internal string Password { get; set; }
    }
}
