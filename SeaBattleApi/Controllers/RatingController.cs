using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeaBattleApi.Auth;
using SeaBattleRepository.Implement;
using SeaBattleRepository.DTO;

namespace SeaBattleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly RepositoryGame repositoryGame;

        public RatingController(RepositoryGame repositoryGame)
        {
            this.repositoryGame = repositoryGame;
        }

        [Authorize]
        [HttpPost("GetRating")]
        public async Task<ActionResult<RatingInfo>> GetRating()
        {
            int userId = HttpContextInfo.GetUserID(this.HttpContext);
            var games = repositoryGame.GetByCondition(s => s.Status == 2 && (s.IdUsers.FirstOrDefault(s => s.Id == userId) != null));
            int count = games.Count();
            int win = games.Where(s => s.IdUserWinner == userId).Count();
            return new RatingInfo { 
                User = userId, 
                Count = count,
                Win = win
            };
        }
    }
}
