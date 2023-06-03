namespace QuizuApi.Exceptions
{
    public class QuizPlayException : Exception
    {
        public QuizPlayException()
        {
        }

        public QuizPlayException(string? message) : base(message)
        {
        }

        public QuizPlayException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
