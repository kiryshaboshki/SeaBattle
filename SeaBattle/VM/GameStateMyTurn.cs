// GameStateMyTurn.cs (обновлённый)
using SeaBattleRepository.DTO;
using SeaBattleWPF.API;
using SeaBattleWPF.API.Game;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeaBattleWPF.VM
{
    internal class GameStateMyTurn : GameState
    {
        private TcpGameClient _client = TcpGameClient.Instance;

        public override void ClickField(Canvas fieldUser, MouseButtonEventArgs e)
        {
            if (fieldUser == fieldUser2)
            {
                var position = e.GetPosition(fieldUser);
                int x = (int)Math.Round(position.X) / 30;
                int y = (int)Math.Round(position.Y) / 30;

                // Проверяем, не стреляли ли уже в эту клетку
                var index = x + y * 10;
                bool alreadyShot = false;
                foreach (var child in fieldUser.Children)
                {
                    if (child is Rectangle rect)
                    {
                        var left = Canvas.GetLeft(rect);
                        var top = Canvas.GetTop(rect);
                        if ((int)(left / 30) == x && (int)(top / 30) == y)
                        {
                            alreadyShot = true;
                            break;
                        }
                    }
                }

                if (alreadyShot) return;

                Task.Run(async () =>
                {
                    await _client.SendAsync("MAKE_TURN", new
                    {
                        GameId = Game.CurrentGame.Id,
                        X = x,
                        Y = y
                    });
                });
            }
        }
    }
}