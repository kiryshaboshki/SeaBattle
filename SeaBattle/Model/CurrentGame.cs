namespace SeaBattle
{
    public static class CurrentGame
    {
        public static int Id { get; set; } = 1;
        public static bool IsOnline { get; set; } = false; // По умолчанию оффлайн для тестирования
        public static string OpponentName { get; set; } = "Компьютер";
        public static string PlayerName { get; set; } = "Игрок";

        // Метод для переключения режима
        public static void SetOfflineMode()
        {
            IsOnline = false;
            OpponentName = "Компьютер";
            Id = 999; // Тестовый ID
        }

        public static void SetOnlineMode(int gameId, string opponent)
        {
            IsOnline = true;
            OpponentName = opponent;
            Id = gameId;
        }
    }
}