namespace MailClient.Constants
{
    public class ImapCommands
    {
        public const string Login = "LOGIN ";
        public const string Logout = "LOGOUT";
        public const string Select = "SELECT ";
        public const string UidSearchAll = "UID SEARCH ALL";
        public const string UidFetch = "UID FETCH {0} (RFC822)";
        public const string Fetch = "FETCH";
        public const string Search = "* SEARCH";
    }
}
