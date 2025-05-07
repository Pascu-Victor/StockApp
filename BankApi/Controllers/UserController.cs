namespace BankApi.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using StockApp.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static readonly List<User> Users = new();

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            // Simulate async
            return await Task.FromResult(Ok(Users));
        }

        [HttpGet("{cnp}")]
        public async Task<ActionResult<User>> GetUserByCnp(string cnp)
        {
            var user = Users.FirstOrDefault(u => u.CNP == cnp);
            if (user == null)
                return NotFound($"User with CNP {cnp} not found.");

            return await Task.FromResult(Ok(user));
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.CNP))
            {
                return BadRequest("Invalid user data.");
            }

            Users.Add(user);
            return await Task.FromResult(CreatedAtAction(nameof(GetUserByCnp), new { cnp = user.CNP }, user));
        }

        [HttpPut("{cnp}")]
        public async Task<ActionResult> UpdateUser(string cnp, [FromBody] User updatedUser)
        {
            var user = Users.FirstOrDefault(u => u.CNP == cnp);
            if (user == null)
                return NotFound($"User with CNP {cnp} not found.");

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;

            return await Task.FromResult(NoContent());
        }

        [HttpDelete("{cnp}")]
        public async Task<ActionResult> DeleteUser(string cnp)
        {
            var user = Users.FirstOrDefault(u => u.CNP == cnp);
            if (user == null)
                return NotFound($"User with CNP {cnp} not found.");

            Users.Remove(user);
            return await Task.FromResult(NoContent());
        }
    }
}
