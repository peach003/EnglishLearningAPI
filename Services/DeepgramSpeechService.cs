using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;

namespace EnglishLearningAPI.Services
{
    public class DeepgramSpeechService
    {
        private const string ApiKey = "YOUR_DEEPGRAM_API_KEY";
        private const string DeepgramUrl = "https://api.deepgram.com/v1/listen?punctuate=true&diarize=true&utterances=true";

        /// <summary>
        /// ✅ 上传音频文件并获取完整字幕
        /// </summary>
        public static async Task<JArray> RecognizeSpeechWithTimestampsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new JArray { new JObject { ["error"] = "File not found." } };
            }

            var client = new RestClient(DeepgramUrl);
            var request = new RestRequest()
                .AddHeader("Authorization", $"Token {ApiKey}")
                .AddHeader("Content-Type", "audio/mpeg");

            request.AddFile("audio", filePath);
            var response = await client.ExecutePostAsync(request);

            if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
            {
                return new JArray { new JObject { ["error"] = "Failed to get response from Deepgram API." } };
            }

            try
            {
                var json = JObject.Parse(response.Content);
                return json["results"]?["utterances"] as JArray ?? new JArray();
            }
            catch (Exception ex)
            {
                return new JArray { new JObject { ["error"] = $"JSON parsing error: {ex.Message}" } };
            }
        }

        /// <summary>
        /// ✅ WebSocket 代理，转发音频流给 Deepgram
        /// </summary>
        public static async Task StreamToDeepgram(WebSocket clientSocket)
        {
            using var deepgramSocket = new ClientWebSocket();
            var deepgramUrl = $"wss://api.deepgram.com/v1/listen?access_token={ApiKey}&punctuate=true";
            await deepgramSocket.ConnectAsync(new Uri(deepgramUrl), CancellationToken.None);

            var receiveBuffer = new byte[4096];
            var sendBuffer = new byte[4096];

            // 🔹 监听前端 WebSocket，获取音频流并转发给 Deepgram
            _ = Task.Run(async () =>
            {
                while (clientSocket.State == WebSocketState.Open)
                {
                    var result = await clientSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await deepgramSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationToken.None);
                        break;
                    }
                    await deepgramSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, result.Count), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            });

            // 🔹 监听 Deepgram WebSocket，接收字幕并返回给前端
            while (deepgramSocket.State == WebSocketState.Open)
            {
                var result = await deepgramSocket.ReceiveAsync(new ArraySegment<byte>(sendBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Deepgram closed", CancellationToken.None);
                    break;
                }

                var transcript = Encoding.UTF8.GetString(sendBuffer, 0, result.Count);
                await clientSocket.SendAsync(new ArraySegment<byte>(sendBuffer, 0, result.Count), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
