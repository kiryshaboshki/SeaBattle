namespace SeaBattleRepository.DTO
{
    public class GameTurn
    {
        public int IdUserNextTurn { get; set; }
        public int IdWinner { get; set; }
        public byte[] FieldUser { get; set; }
    }
}
