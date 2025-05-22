namespace MailClient.Constants
{
    public class SmtpCommands
    {
        public const string MailFrom = "MAIL FROM:";
        public const string RcptTo = "RCPT TO:";
        public const string Data = "DATA";
        public const string Quit = "DATA";
        public const string AuthPlain = "AUTH PLAIN ";
    }

    public class ImapCommands
    {
        public const string Login = "LOGIN ";
        public const string Logout = "LOGOUT";
        public const string Select = "SELECT  ";

    }
}
