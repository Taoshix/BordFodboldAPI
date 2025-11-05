namespace BordFodboldAPI.Exceptions
{
    public class PlayerOnBothTeamsException : Exception
    {
        public PlayerOnBothTeamsException()
        {

        }
        public PlayerOnBothTeamsException(string message) : base(message)
        {

        }
        public PlayerOnBothTeamsException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
