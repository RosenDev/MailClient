using System.Text;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Services
{
    /// <summary>
    /// Reads user input from the console, supporting both plain text and masked fields.
    /// </summary>
    public class ConsoleInputReaderService : IInputReaderService
    {
        /// <summary>
        /// Reads a line of text from the console.
        /// </summary>
        /// <returns>
        /// The line entered by the user, or null if no input is available.
        /// </returns>
        public string? ReadLine()
        {
            return Console.ReadLine();
        }

        /// <summary>
        /// Reads a secure field (e.g., a password) from the console, masking input with '*'.
        /// </summary>
        /// <returns>The unmasked string entered by the user.</returns>
        public string ReadSecureField()
        {
            var secureField = new StringBuilder();
            ConsoleKeyInfo key;
            while((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                if(key.Key == ConsoleKey.Backspace && secureField.Length > 0)
                {
                    secureField.Length--;
                    Console.Write("\b \b");
                }
                else if(!char.IsControl(key.KeyChar))
                {
                    secureField.Append(key.KeyChar);
                    Console.Write('*');
                }
            }
            return secureField.ToString();
        }
    }
}
