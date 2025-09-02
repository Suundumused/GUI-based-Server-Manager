namespace Server_Manager_Application.Models.Security
{
    public class UserState()
    {
        public Int16 _cAttempts { get; set; }
        public CancellationTokenSource _TokenSource { get; } = new CancellationTokenSource();

        public UserState(Int16 cAttempts) : this()
        {
            _cAttempts = cAttempts;
        }
    }
}
