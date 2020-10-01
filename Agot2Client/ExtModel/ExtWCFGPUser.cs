namespace GamePortal
{
    static public class ExtWCFUserWCFUser
    {
        static public void Update(this WCFUser o, WCFUser newValue)
        {
            o.AllPower = newValue.AllPower;
            o.IsIgnore = newValue.IsIgnore;
            o.Login = newValue.Login;
            o.LastConnection = newValue.LastConnection;
            o.Power = newValue.Power;
            o.SpecialUsers = newValue.SpecialUsers;
            o.Title = newValue.Title;
            o.UserLikes = newValue.UserLikes;
            o.Version = newValue.Version;
            o.LastPayment = newValue.LastPayment;
            o.UserGames = newValue.UserGames;
            o.Api = newValue.Api;
        }
    }
}
