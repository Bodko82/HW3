using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
using CommonLibrary;
using CommonLibrary.Requests;
using CommonLibrary.Responses;

namespace Client
{
    public class Service
    {
        private IPEndPoint _ep;

        public Service(string host, int port)
        {
            _ep = new IPEndPoint(IPAddress.Parse(host), port);
        }

        private Data Send(Data request)
        {
            using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
            {
                s.Connect(_ep);
                s.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)));

                var buffer = new byte[4096];
                int read = s.Receive(buffer);

                string incoming = Encoding.UTF8.GetString(buffer, 0, read);
                return JsonSerializer.Deserialize<Data>(incoming);
            }
        }

        private T ToResponse<T>(Data response) where T : class
        {
            return JsonSerializer.Deserialize<T>(response.Content);
        }

        public bool Authenticate(string login, string password)
        {
            var response = ToResponse<LoginResponse>(
                Send(Data.Create(new LoginRequest
                {
                    Me = new CommonLibrary.Client
                    {
                        Login = login,
                        Password = password
                    }
                })));

            return response.Success;
        }

        public int GetMessageCount(string login)
        {
            var response = ToResponse<LoginResponse>(
                Send(Data.Create(new LoginRequest
                {
                    Me = new CommonLibrary.Client
                    {
                        Login = login
                    }
                })));

            return response.MessagesCount;
        }

        public List<Message> GetMessages(string login)
        {
            var response = ToResponse<GetMessagesResponse>(
                Send(Data.Create(new GetMessagesRequest
                {
                    Me = new CommonLibrary.Client
                    {
                        Login = login
                    }
                })));

            return response.Messages;
        }

        public bool SendMessage(Message message)
        {
            var response = ToResponse<SendMessageResponse>(
                Send(Data.Create(new SendMessageRequest
                {
                    Message = message
                })));

            return response.Success;
        }

        public bool Register(string login, string password)
        {
            string configFilePath = "config.json";

            if (!File.Exists(configFilePath))
            {
                File.Create(configFilePath).Dispose();
                File.WriteAllText(configFilePath, "{}");
            }

            string configJson = File.ReadAllText(configFilePath);
            var configData = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);

            if (configData.ContainsKey(login))
            {
                return false;
            }

            configData.Add(login, password);
            string updatedConfigJson = JsonSerializer.Serialize(configData);
            File.WriteAllText(configFilePath, updatedConfigJson);

            return true;
        }
    }
}
