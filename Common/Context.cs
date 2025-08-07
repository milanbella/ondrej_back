namespace Ondrej.Common
{
    public class Context
    {
        public static string HTTP_CONTEXT_KEY_TOKEN_CLAIMS = "token_claims";
        public static string HTTP_CONTEXT_KEY_SESSION_ID = "session_id";


        public static int TOKEN_EXPIRATION_HOURS = 100 * 365 * 24; // 100 years

        // debugging
        public static bool IS_DEBUG = false;

    }
}
