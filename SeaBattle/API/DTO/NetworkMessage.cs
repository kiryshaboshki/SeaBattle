using System.Text.Json.Serialization;

namespace SeaBattleWPF.API
{
    public class NetworkMessage<T>
    {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class AuthRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class CreateGameRequest
    {
        // Можно добавить настройки игры
    }

    public class JoinGameRequest
    {
        public int GameId { get; set; }
    }

    public class MakeTurnRequest
    {
        public int GameId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ReadyRequest
    {
        public int GameId { get; set; }
        public byte[] Field { get; set; }
    }
}