namespace MailClient.App.Infrastructure.Contracts
{
    public interface IPrompt<T>
    {
        /// <summary> Show the prompt, read and return a T (e.g. string, Credential, MailMessage, etc.) </summary>
        T Run();
    }

}
