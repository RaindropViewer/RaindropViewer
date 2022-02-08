using System.Collections.Generic;
using UniRx;

namespace Raindrop.Presenters
{
    public interface ILoginView
    {
        string Username { get; set; }
        string Password { get; set; }


        bool isSaveCredentials { get; }
        bool agreeTOS { get; }

        int loginLocation { get; }
        
        
    }
}