using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Ondrej.Sessionn // use nn to avoid conflicts
{
    public class SessionService
    {
        private static string CLASS_NAME = typeof(SessionService).Name;
        private Dbo.Db db;
        private HttpContext context;

        public SessionService(Dbo.Db db, HttpContext context)
        {
            // Constructor can be used for initialization if needed
            this.db = db;
            this.context = context;
        }

        public long? GetSessionId()
        {
            const string METHOD_NAME = "GetSessionId()";

            if (context.Items.ContainsKey(Ondrej.Common.Context.HTTP_CONTEXT_KEY_SESSION_ID))
            {
                if (context.Items[Ondrej.Common.Context.HTTP_CONTEXT_KEY_SESSION_ID] is long sessionId)
                {
                    return sessionId;
                }
                else
                {
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME} - Session ID is not a string");
                    throw new Exception("Session ID is not a string");
                }
            }
            else
            {
                return null; // No session ID found in context
            }
        }

        public async Task<Ondrej.Dbo.Model.User?> GetLoggedInUser()
        {
            try
            {
                long? sessionDbId = GetSessionId();
                /*
                if (sessionId == null)
                {
                    return null;
                }
                long? sessionDbId = await db.Session
                    .Where(s => s.SessionId == sessionId)
                    .Select(s => (long?)s.Id)
                    .FirstOrDefaultAsync();
                if (sessionDbId == null)
                {
                    return null;
                }
                */

                var sessionUser = await db.SessionUser
                    .Include(su => su.User)
                    .FirstOrDefaultAsync(su => su.SessionId == sessionDbId);
                if (sessionUser != null && sessionUser.User != null)
                {
                    return sessionUser.User;
                }
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:GetLoggedInUser() - Error retrieving logged in user");
                return null;
            }
        }

        /*
        public async Task<bool> GetBarcodeJwt(string token)
        {
            const string METHOD_NAME = "GetJwtToken()";

            try
            {
                var tokenExists = await db.JWT.AnyAsync(j => j.Token.Equals(token));

                if (tokenExists)
                {
                    return true;
                }
                else
                {
                    Log.Error($"{token} token doesn't exist in the database");
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME} - Error validating input JWT");
                throw;
            }
        }
        */

        public async Task<int> GetUserId()
        {
            const string METHOD_NAME = "GetUserId()";

            try
            {
                long? sessionId = GetSessionId();

                // retrieve only the user ID from SessionUser table
                var userId = await db.SessionUser
                    .Where(su => su.SessionId == sessionId)
                    .Select(su => (int?)su.UserId)
                    .FirstOrDefaultAsync();

                if (userId == null)
                {
                    Log.Error($"{CLASS_NAME}:{METHOD_NAME} - No user found for session ID {sessionId}");
                    throw new Exception("No user found for session ID");
                }
                return userId.Value;
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME} - Error retrieving user ID");
                throw;
            }
        }

        public void RemoveSession(long sessionId)
        {
            const string METHOD_NAME = "RemoveSession()";

            try
            {
                // Remove all SessionUser entries for this session
                var sessionUsers = db.SessionUser.Where(su => su.SessionId == sessionId).ToList();
                db.SessionUser.RemoveRange(sessionUsers);

                // Remove the session itself
                var session = db.Session.FirstOrDefault(s => s.Id == sessionId);
                if (session != null)
                {
                    db.Session.Remove(session);
                }

                // Save changes to the database
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Log.Error(e, $"{CLASS_NAME}:{METHOD_NAME} - Error removing session");
            }
        }
    }
}