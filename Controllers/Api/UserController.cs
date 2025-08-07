using Microsoft.AspNetCore.Mvc;
using Ondrej.Controllers.Model.Api.UserController;
using Ondrej.Sessionn;

namespace Ondrej.Controllers.Api
{
    [Route("api/user")]
    public class UserController: Controller
    {
        private SessionService sessionService;

        public UserController(SessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet("get-user")]
        public async Task<ActionResult<GetUserResponse>> GetUser()
        {
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }
            var getUserResponse = new GetUserResponse(user.Name, user.Email, user.FirstName, user.LastName);

            return Ok(getUserResponse);
        }
    }
}
