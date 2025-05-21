namespace CreditSystem.API.Exceptions
{
    public class CreditAuthorizationException : Exception
    {
        public CreditAuthorizationException(string message) : base(message) { }
    }
}
