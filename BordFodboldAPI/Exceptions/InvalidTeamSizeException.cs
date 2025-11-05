namespace BordFodboldAPI.Exceptions
{
    public class InvalidTeamSizeException : Exception
    {
        public InvalidTeamSizeException()
        {

        }

        public InvalidTeamSizeException(string message) : base(message)
        {

        }

        public InvalidTeamSizeException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
