using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizuApi.Data;
using QuizuApi.Models;

namespace QuizuApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly QuizuApiDbContext _context;

        public AuthController(UserManager<User> userManager, QuizuApiDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Temp()
        {
            var user = new User
            {
                UserName = "Krezlau",
                Email = "email1@e.pl",
                JoinedAt = DateTime.Now,
                Name = "imie",
                Surname = "nazwisko",
                Location = "Poland"
            };
            await _userManager.CreateAsync(user, "Cebula123!@#");

            var user2 = new User
            {
                UserName = "userOne",
                Email = "email2@e.pl",
                JoinedAt = DateTime.Now,
                Name = "imie",
                Surname = "nazwisko",
                Location = "Poland"
            };
            await _userManager.CreateAsync(user2, "Cebula123!@#");

            var user3 = new User
            {
                UserName = "userTwo",
                Email = "email3@e.pl",
                JoinedAt = DateTime.Now,
                Name = "imie",
                Surname = "nazwisko",
                Location = "Poland"
            };
            await _userManager.CreateAsync(user3, "Cebula123!@#");

            user.Followers.Add(user2);
            user.Followers.Add(user3);
            user.Following.Add(user2);

            _context.UserFollows.Add(new UserFollow()
            {
                UserFollowedId = user.Id,
                UserFollowingId = user2.Id
            });
            _context.UserFollows.Add(new UserFollow()
            {
                UserFollowedId = user.Id,
                UserFollowingId = user3.Id
            });
            _context.UserFollows.Add(new UserFollow()
            {
                UserFollowedId = user2.Id,
                UserFollowingId = user.Id,
            });

            _context.Update(user);
            _context.SaveChanges();

            return Ok();
        }

        [HttpGet("xd")]
        public async Task<IActionResult> Gettt()
        {
            var users = _context.Users.Where(x => x.Id != "").Include("Followers").Include("Following").ToList();

            return Ok(users);
    }
    }

    
}
