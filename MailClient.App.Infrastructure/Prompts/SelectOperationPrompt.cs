using MailClient.App.Infrastructure.Constants;
using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.Prompts
{
    /// <summary>
    /// Displays available operations after authentication and prompts the user to choose one.
    /// Validates input and supports 'Cancel' to return to the main menu.
    /// </summary>
    public class SelectOperationPrompt : IPrompt<Operation>
    {
        private readonly IInputReaderService _inputReaderService;
        private readonly IOutputWriterService _outputWriterService;

        public SelectOperationPrompt(
            IInputReaderService inputReaderService,
            IOutputWriterService outputWriterService)
        {
            _inputReaderService = inputReaderService;
            _outputWriterService = outputWriterService;
        }

        public Operation Run()
        {
            while(true)
            {
                _outputWriterService.WriteLine("Available operations:");
                var operations = Enum.GetValues(typeof(Operation)).Cast<Operation>().ToList();
                for(int i = 0; i < operations.Count; i++)
                {
                    _outputWriterService.WriteLine($"  {i + 1}. {operations[i]}");
                }
                _outputWriterService.Write("Enter the number of the operation you want to perform: ");

                var input = _inputReaderService.ReadLine()?.Trim() ?? string.Empty;

                if(int.TryParse(input, out int index) && index >= 1 && index <= operations.Count)
                {
                    return operations[index - 1];
                }

                if(Enum.TryParse<Operation>(input, true, out var op))
                {
                    return op;
                }

                _outputWriterService.WriteLine("Invalid selection. Please enter a valid number or operation name, or 'Cancel'.");
            }
        }
    }
}
