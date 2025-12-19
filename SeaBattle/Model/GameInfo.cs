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

        public string DisplayText => $"{CreatorName} | {StartTime:HH:mm} | {Status} | {PlayersCount}/{MaxPlayers} игроков";

        public string DetailedInfo
        {
            get
            {
                var info = $"{CreatorName}\n";
                info += $"Начало: {StartTime:HH:mm}\n";
                info += $"Статус: {Status}\n";
                info += $"Игроков: {PlayersCount}/{MaxPlayers}";

                if (!string.IsNullOrEmpty(Winner))
                    info += $"\nПобедитель: {Winner}";

                return info;
            }
        }
    }
}