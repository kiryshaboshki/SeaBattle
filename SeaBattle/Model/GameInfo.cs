using System;

namespace SeaBattle.Models
{
    public class GameInfo
    {
        public int Id { get; set; }
        public string CreatorName { get; set; }
        public DateTime StartTime { get; set; }
        public string Status { get; set; }

        public string DisplayText => $"{CreatorName} - {StartTime:HH:mm} ({Status})";
    }
}