namespace TerminalGateway.ApiService
{
    public class ProblemException :Exception
    {

        public string Error { get; }

        public string Message { get; }

        public ProblemException(string error, string message):base(message)
        {
            Error = error;
            Message = message;
        }
    }

    public class ProblemExceptionHandler
    {

    }
}
