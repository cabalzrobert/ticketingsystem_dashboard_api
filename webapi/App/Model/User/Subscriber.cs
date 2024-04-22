using System;

namespace webapi.App.Model.User
{
    public class Subscriber : User 
    {
        public String SignatureID;
        public bool IsTerminal;
        public String PasswordExpired;
        public bool IsGeneralCoordinator;
        public bool IsCoordinator;
        public bool IsPlayer;
    }
}
