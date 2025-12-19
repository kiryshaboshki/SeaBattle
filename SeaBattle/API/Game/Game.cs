// Game.cs (упрощённый, без HTTP)
using SeaBattleRepository.DTO;
using SeaBattleWPF.VM;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeaBattleWPF.API.Game
{
    static internal class Game
    {
        static GameState currentState;
        internal static GameDTO CurrentGame;
        internal static bool CreatorIsCurrentUser;

        static Dictionary<States, GameState> states;

        static Game()
        {
            states = new()
            {
                { States.WaitJoin, new GameStateWaitOpponentToJoin() },
                { States.WaitTurn, new GameStateWaitOpponentTurn() },
                { States.MyTurn, new GameStateMyTurn() }
            };
        }

        internal static GameState GetState(States key)
        {
            return states[key];
        }

        internal static void SetState(States key)
        {
            currentState = states[key];
        }

        static internal void RegisterField(Canvas fieldUser, bool currentUser)
        {
            foreach (var state in states.Values)
                state.RegisterField(fieldUser, currentUser);

            RedrawMyField(fieldUser, new byte[100]);
        }

        static public void RedrawMyField(Canvas fieldUserCanvas, byte[] fieldUser)
        {
            fieldUserCanvas.Children.Clear();
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    var index = i + j * 10;
                    Rectangle box = new Rectangle();
                    box.Width = 30;
                    box.Height = 30;
                    Brush hit = Brushes.Blue; // море
                    switch (fieldUser[index])
                    {
                        case 1: hit = Brushes.Gray; break; // корабль
                        case 2: hit = Brushes.Red; break; // попадание
                        case 3: hit = Brushes.White; break; // промах
                        case 4: hit = Brushes.Black; break; // убитый корабль
                    }
                    box.Fill = hit;
                    Canvas.SetLeft(box, i * 30);
                    Canvas.SetTop(box, j * 30);
                    fieldUserCanvas.Children.Add(box);
                }
        }

        internal static void TestTurn(GameTurn turn)
        {
            currentState = currentState.TestTurn(turn);
        }

        internal static void ClickField(Canvas fieldUser, MouseButtonEventArgs e)
        {
            currentState.ClickField(fieldUser, e);
        }
    }
}