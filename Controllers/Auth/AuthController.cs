using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ondrej.Auth;
using Ondrej.Controllers.Model.Auth.AuthController;
using Ondrej.Dbo;
using Ondrej.Dbo.Model;
using Ondrej.Sessionn;
using Serilog;
using User = Ondrej.Dbo.Model.User;

namespace Ondrej.Controllers.Auth
{
    [Route("auth")]
    public class AuthController : Controller
    {
        public static string CLASS_NAME = typeof(AuthController).Name;

        private readonly SessionService sessionService;

        public AuthController(SessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        public async Task<IActionResult> Index()
        {
            // Simulate async operation if needed, e.g., await Task.CompletedTask;
            return await Task.FromResult(View());
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
            GetUserResponse getUserResponse;

            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                getUserResponse = new GetUserResponse(false, null, null, null, null);
                return Ok(getUserResponse);
            }
            getUserResponse = new GetUserResponse(true, user.Name, user.Email, user.FirstName, user.LastName);

            return Ok(getUserResponse);
        }

        [HttpPost("browser-register")]
        public async Task<ActionResult<BrowserRegisterResponse>> BrowserRegister([FromBody] BrowserRegisterRequest request,
        [FromServices] Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "BrowserRegister()";

            using var transaction = db.Database.BeginTransaction();
            try
            {

                if (string.IsNullOrEmpty(request.username))
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "missing username",
                        message: "missing username"
                    )));
                }

                if (string.IsNullOrEmpty(request.email))
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "missing email",
                        message: "missing email"
                    )));
                }

                if (string.IsNullOrEmpty(request.password))
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "missing password",
                        message: "missing password"
                    )));
                }

                if (string.IsNullOrEmpty(request.passwordVerify))
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "missing passwordVerify",
                        message: "missing passwordVerify"
                    )));
                }

                if (request.password != request.passwordVerify)
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "passwords do not match",
                        message: "passwords do not match"
                    )));
                }

                // Find user by username
                var user = await db.User.FirstOrDefaultAsync(u => u.Name == request.username);
                if (user != null)
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "user_exists",
                        message: "such a user already exists"
                    )));
                }

                // Find user by email
                user = await db.User.FirstOrDefaultAsync(u => u.Email == request.email);
                if (user != null)
                {
                    return await Task.FromResult(StatusCode(400, new BrowserLoginResponse(
                        error: "email_exists",
                        message: "such a user already exists"
                    )));
                }


                // Get session ID using SessionService
                long? sessionDbId = sessionService.GetSessionId();
                if (sessionDbId == null)
                {
                    // SessionMiddleware should have set the session ID in the context
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME} - Session ID is null");
                    return await Task.FromResult(StatusCode(500, new BrowserLoginResponse(
                        error: "server_error",
                        message: "Session ID is not set."
                    )));
                }

                // Remove existing SessionUser entries for this session if any exist
                var existingSessionUsers = await db.SessionUser.Where(su => su.SessionId == sessionDbId).ToListAsync();
                if (existingSessionUsers.Any())
                {
                    db.SessionUser.RemoveRange(existingSessionUsers);
                    await db.SaveChangesAsync();
                }

                // Add entry in SessionUser table
                var sessionUser = new SessionUser
                {
                    UserId = user.Id,
                    SessionId = sessionDbId,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddYears(1)
                };
                db.SessionUser.Add(sessionUser);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return await Task.FromResult(Ok(new BrowserLoginResponse(
                    error: "",
                    message: "Login successful."
                )));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - Error during login process");
                return await Task.FromResult(StatusCode(500, new BrowserLoginResponse(
                    error: "server_error",
                    message: "An error occurred during login."
                )));
            }
        }

        [HttpPost("browser-login")]
        public async Task<ActionResult<BrowserLoginResponse>> BrowserLogin([FromBody] BrowserLoginRequest request,
        [FromServices] Db db, [FromServices] SessionService sessionService)
        {
            const string METHOD_NAME = "BrowserLogin()";

            using var transaction = db.Database.BeginTransaction();
            try
            {

                if (request == null || string.IsNullOrEmpty(request.username) || string.IsNullOrEmpty(request.password))
                {
                    return await Task.FromResult(StatusCode(401, new BrowserLoginResponse(
                        error: "unauthorized",
                        message: "Username or password is missing."
                    )));
                }

                // Find user by username
                var user = await db.User.FirstOrDefaultAsync(u => u.Name == request.username);
                if (user == null || string.IsNullOrEmpty(user.PasswordSalt) || string.IsNullOrEmpty(user.PasswordHash))
                {
                    return await Task.FromResult(StatusCode(401, new BrowserLoginResponse(
                        error: "unauthorized",
                        message: "Invalid username or password."
                    )));
                }

                // Verify password
                bool valid = Password.verifyPassword(user.PasswordSalt, user.PasswordHash, request.password);
                if (!valid)
                {
                    return await Task.FromResult(StatusCode(401, new BrowserLoginResponse(
                        error: "unauthorized",
                        message: "Invalid username or password."
                    )));
                }

                // Get session ID using SessionService
                long? sessionDbId = sessionService.GetSessionId();
                if (sessionDbId == null)
                {
                    // SessionMiddleware should have set the session ID in the context
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME} - Session ID is null");
                    return await Task.FromResult(StatusCode(500, new BrowserLoginResponse(
                        error: "server_error",
                        message: "Session ID is not set."
                    )));
                }

                // Remove existing SessionUser entries for this session if any exist
                var existingSessionUsers = await db.SessionUser.Where(su => su.SessionId == sessionDbId).ToListAsync();
                if (existingSessionUsers.Any())
                {
                    db.SessionUser.RemoveRange(existingSessionUsers);
                    await db.SaveChangesAsync();
                }

                // Add entry in SessionUser table
                var sessionUser = new SessionUser
                {
                    UserId = user.Id,
                    SessionId = sessionDbId,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddYears(1)
                };
                db.SessionUser.Add(sessionUser);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return await Task.FromResult(Ok(new BrowserLoginResponse(
                    error: "",
                    message: "Login successful."
                )));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - Error during login process");
                return await Task.FromResult(StatusCode(500, new BrowserLoginResponse(
                    error: "server_error",
                    message: "An error occurred during login."
                )));
            }
        }

        [HttpPost("device-login")]
        public async Task<ActionResult<DeviceLoginResponse>> DeviceLogin(
            [FromBody] DeviceLoginRequest request,
            [FromServices] Db db,
            [FromServices] SessionService sessionService,
            [FromServices] TokenService tokenService)
        {
            const string METHOD_NAME = "DeviceLogin()";

            using var transaction = db.Database.BeginTransaction();
            try
            {
                var deviceIdentification = request.deviceIdentification;


                // Find user by username
                var user = await db.User.FirstOrDefaultAsync(u => u.Email == request.userEmail);
                if (user == null || string.IsNullOrEmpty(user.PasswordSalt) || string.IsNullOrEmpty(user.PasswordHash))
                {
                    return await Task.FromResult(StatusCode(401, new BrowserLoginResponse(
                        error: "unauthorized",
                        message: "Invalid username or password."
                    )));
                }

                // Verify password
                bool valid = Password.verifyPassword(user.PasswordSalt, user.PasswordHash, request.password);
                if (!valid)
                {
                    return await Task.FromResult(StatusCode(401, new BrowserLoginResponse(
                        error: "unauthorized",
                        message: "Invalid username or password."
                    )));
                }

                // ensure device and session exist
                var device = await db.Device.FirstOrDefaultAsync(d => d.DeviceId == deviceIdentification);
                if (device == null)
                {
                    device = new Device
                    {
                        DeviceId = deviceIdentification,
                        RegisteredAt = DateTime.Now,
                    };
                    db.Device.Add(device);
                    await db.SaveChangesAsync();

                    var session = new Session
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddYears(1),
                        AccessedAt = DateTime.Now
                    };
                    db.Session.Add(session);
                    await db.SaveChangesAsync();

                    // Link session to device
                    device.Session = session;
                    db.Device.Update(device);
                    await db.SaveChangesAsync();

                    device = await db.Device.FirstOrDefaultAsync(d => d.DeviceId == deviceIdentification);
                }
                else
                {
                    // Create new session
                    var newSession = new Session
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddYears(1),
                        AccessedAt = DateTime.Now
                    };
                    db.Session.Add(newSession);
                    await db.SaveChangesAsync();

                    // Link session to device
                    device.Session = newSession;
                    await db.SaveChangesAsync();
                }

                long sessionId = device.SessionId.Value;

                // Create or update SessionUser entry
                var existingSessionUser = await db.SessionUser
                    .FirstOrDefaultAsync(su => su.SessionId == sessionId && su.UserId == user.Id);
                if (existingSessionUser == null)
                {
                    var sessionUser = new SessionUser
                    {
                        UserId = user.Id,
                        SessionId = sessionId,
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddYears(1)
                    };
                    db.SessionUser.Add(sessionUser);
                }
                else
                {
                    existingSessionUser.CreatedAt = DateTime.Now;
                    existingSessionUser.ExpiresAt = DateTime.Now.AddYears(1);
                    db.SessionUser.Update(existingSessionUser);
                }

                string jwt = tokenService.GenerateJWT(
                    db,
                    user.Name,
                    user.Id,
                    device.Id,
                    device.DeviceId,
                    100L * 365 * 24 * 3600, // 100 years validity
                    Ondrej.Auth.Token.UserType.RegisteredUser
                );

                await transaction.CommitAsync();
                return await Task.FromResult(Ok(new DeviceLoginResponse(
                    error: "",
                    message: "Login successful.",
                    jwt: jwt
                )));

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - Error during login process");
                return await Task.FromResult(StatusCode(500, new BrowserLoginResponse(
                    error: "server_error",
                    message: "An error occurred during login."
                )));
            }
        }

        [HttpPost("device-register")]
        public async Task<ActionResult<DeviceRegisterResponse>> DeviceRegister(
            [FromBody] DeviceRegisterRequest request,
            [FromServices] Db db,
            [FromServices] SessionService sessionService,
            [FromServices] TokenService tokenService)
        {
            const string METHOD_NAME = "DeviceRegister()";

            using var transaction = db.Database.BeginTransaction();
            try
            {
                // Field-by-field validation
                if (request == null)
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "Request body is missing.",
                        jwt: ""
                    )));
                }

                if (string.IsNullOrEmpty(request.deviceIdentification))
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "Device identification is required.",
                        jwt: ""
                    )));
                }

                if (string.IsNullOrEmpty(request.email))
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "Email is required.",
                        jwt: ""
                    )));
                }

                if (string.IsNullOrEmpty(request.password))
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "Password is required.",
                        jwt: ""
                    )));
                }

                if (string.IsNullOrEmpty(request.firstName))
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "First name is required.",
                        jwt: ""
                    )));
                }

                if (string.IsNullOrEmpty(request.lastName))
                {
                    return await Task.FromResult(StatusCode(400, new DeviceRegisterResponse(
                        error: "bad_request",
                        message: "Last name is required.",
                        jwt: ""
                    )));
                }

                // Check if user already exists with the same email
                var existingUser = await db.User.FirstOrDefaultAsync(u => u.Email == request.email);
                if (existingUser != null)
                {
                    return await Task.FromResult(StatusCode(409, new DeviceRegisterResponse(
                        error: "conflict",
                        message: "User with this email already exists.",
                        jwt: ""
                    )));
                }

                // Create new user
                var encodedPassword = Password.getEncodedPassword(request.password);
                var newUser = new User
                {
                    Email = request.email,
                    Name = request.email.Split('@')[0], // Use part of email as username
                    FirstName = request.firstName,
                    LastName = request.lastName,
                    PasswordSalt = encodedPassword.PasswordSalt,
                    PasswordHash = encodedPassword.PasswordHash,
                    IsEmailVerified = false,
                    Country = "Unknown",
                    Language = "en"
                };

                db.User.Add(newUser);
                await db.SaveChangesAsync();
                // Device handling - same as DeviceLogin
                var deviceIdentification = request.deviceIdentification;
                var device = await db.Device.FirstOrDefaultAsync(d => d.DeviceId == deviceIdentification);
                if (device == null)
                {
                    device = new Device
                    {
                        DeviceId = deviceIdentification,
                        RegisteredAt = DateTime.Now,
                    };
                    db.Device.Add(device);
                    await db.SaveChangesAsync();

                    var session = new Session
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddYears(1),
                        AccessedAt = DateTime.Now
                    };
                    db.Session.Add(session);
                    await db.SaveChangesAsync();

                    // Link session to device
                    device.Session = session;
                    db.Device.Update(device);
                    await db.SaveChangesAsync();

                    device = await db.Device.FirstOrDefaultAsync(d => d.DeviceId == deviceIdentification);

                }
                else
                {
                    // Create new session
                    var newSession = new Session
                    {
                        SessionId = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddYears(1),
                        AccessedAt = DateTime.Now
                    };
                    db.Session.Add(newSession);
                    await db.SaveChangesAsync();

                    // Link session to device
                    device.Session = newSession;
                    await db.SaveChangesAsync();
                }

                long sessionId = device.Session.Id;

                var sessionUser = new SessionUser
                {
                    UserId = newUser.Id,
                    SessionId = sessionId,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddYears(1)
                };
                db.SessionUser.Add(sessionUser);
                await db.SaveChangesAsync();

                // Generate JWT token
                string jwt = tokenService.GenerateJWT(
                    db,
                    newUser.Name,
                    newUser.Id,
                    device.Id,
                    device.DeviceId,
                    100L * 365 * 24 * 3600, // 100 years validity
                    Ondrej.Auth.Token.UserType.RegisteredUser
                );

                await transaction.CommitAsync();
                return await Task.FromResult(Ok(new DeviceRegisterResponse(
                    error: "",
                    message: "Registration successful.",
                    jwt: jwt
                )));
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME} - Error during registration process");
                return await Task.FromResult(StatusCode(500, new DeviceRegisterResponse(
                    error: "server_error",
                    message: "An error occurred during registration.",
                    jwt: ""
                )));
            }
        }

    }
}
