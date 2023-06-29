using CommonLibrary;
using CommonLibrary.Requests;
using CommonLibrary.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Server
{
    public class Core
    {
        private List<Message> _messages = new List<Message>();

        private T Deserialize<T>(Data data) where T : class
        {
            return JsonSerializer.Deserialize<T>(data.Content);
        }

        private Data HandleLoginRequest(Data data)
        {
            var request = Deserialize<LoginRequest>(data);

            string login = request.Me.Login;
            string password = request.Me.Password;

            bool isAuthenticated = AuthenticateUser(login, password);

            if (isAuthenticated)
            {
                return Data.Create(new LoginResponse
                {
                    Success = true,
                    MessagesCount = _messages.Where(x => x.To.Login == login).Count(),
                });
            }
            else
            {
                return Data.Create(new LoginResponse
                {
                    Success = false,
                    MessagesCount = 0,
                });
            }
        }

        private bool AuthenticateUser(string login, string password)
        {
            string configFilePath = "config.json";

            if (!File.Exists(configFilePath))
            {
                File.WriteAllText(configFilePath, "{}");
            }
            string configJson = File.ReadAllText(configFilePath);
            var configData = JsonSerializer.Deserialize<Dictionary<string, string>>(configJson);

            if (configData.ContainsKey(login))
            {
                string storedPassword = configData[login];
                return password == storedPassword;
            }

            configData.Add(login, password);

            string updatedConfigJson = JsonSerializer.Serialize(configData);

            File.WriteAllText(configFilePath, updatedConfigJson);

            return true; 
        }

        private Data HandleSendMessageRequest(Data data)
        {
            var request = Deserialize<SendMessageRequest>(data);

            _messages.Add(request.Message);
            return Data.Create(new SendMessageResponse { Success = true });
        }

        private Data HandleGetMessagesRequest(Data data)
        {
            var request = Deserialize<GetMessagesRequest>(data);

            var clientMessages = _messages.Where(x => x.To.Login == request.Me.Login).ToList();
            _messages.RemoveAll(x => x.To.Login == request.Me.Login);

            return Data.Create(new GetMessagesResponse
            {
                Messages = clientMessages
            });
        }

        public Data Handle(Data request)
        {
            switch (request.Type)
            {
                case DataType.LoginRequest:
                    return HandleLoginRequest(request);
                case DataType.SendMessageRequest:
                    return HandleSendMessageRequest(request);
                case DataType.GetMessageRequest:
                    return HandleGetMessagesRequest(request);
                default:
                    return Data.Create(new ErrorResponse
                    {
                        Error = "Unknown request"
                    });
            }
        }
    }
}
