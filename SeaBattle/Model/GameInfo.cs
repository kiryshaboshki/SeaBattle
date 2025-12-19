using System;

namespace SeaBattle.Models
{
    public class GameInfo
    {
        public int Id { get; set; }
        public string CreatorName { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; }
        public int PlayersCount { get; set; }
        public int MaxPlayers { get; set; }
        public string Winner { get; set; }
        public int CreatorRating { get; set; }

        public string DisplayText => $"{Status} | {CreatorName} ⭐{CreatorRating} | {StartTime:HH:mm} | {PlayersCount}/{MaxPlayers}";

        public string DetailedInfo
        {
            get
            {
                var info = $"🎮 Игра #{Id}\n";
                info += $"👤 Создатель: {CreatorName}\n";
                info += $"⭐ Рейтинг: {CreatorRating}\n";
                info += $"🕐 Начало: {StartTime:HH:mm}\n";
                info += $"📊 Статус: {Status}\n";
                info += $"👥 Игроков: {PlayersCount}/{MaxPlayers}";

                if (!string.IsNullOrEmpty(Winner))
                    info += $"\n🏆 Победитель: {Winner}";

                return info;
            }
        }

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    string s when s.Contains("Ожидает") => "Green",
                    string s when s.Contains("В процессе") => "Orange",
                    string s when s.Contains("Завершена") => "Gray",
                    _ => "Black"
                };
            }
        }
    }
}