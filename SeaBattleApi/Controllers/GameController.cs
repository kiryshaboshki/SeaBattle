using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeaBattleApi.Auth;
using SeaBattleLogic;

namespace SeaBattleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameLogic gameLogic;

        public GameController(GameLogic gameLogic) 
        {
            this.gameLogic = gameLogic;
        }

        [Authorize]
        [HttpPost("IsMyTurn")]
        public async Task<ActionResult<GameTurn>> IsMyTurn(int idGame)
        {
            int userId = HttpContextInfo.GetUserID(this.HttpContext);
            return await gameLogic.CheckTurnAsync(userId, idGame);
        }

        [Authorize]
        [HttpPost("MakeTurn")]
        public async Task<ActionResult<TurnResult>> MakeTurn(int idGame, int x, int y)
        {
            int userId = HttpContextInfo.GetUserID(this.HttpContext);
            return await gameLogic.MakeTurnAsync(userId, idGame, x, y);
        }
    }
}
