// User.cs (упрощённый)
namespace SeaBattleWPF.API.Game
{
    internal class User
    {
        public int Id { get; set; }
        public string Login { get; set; }

        // Токены больше не нужны, авторизация через TCP
    }
}