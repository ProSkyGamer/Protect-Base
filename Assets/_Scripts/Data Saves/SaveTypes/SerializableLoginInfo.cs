public class SerializableLoginInfo
{
    public int Login;
    public string Password;

    public SerializableLoginInfo()
    {
    }

    public SerializableLoginInfo(LoginInfo loginInfo)
    {
        Login = loginInfo.Login;
        Password = loginInfo.Password;
    }

    public LoginInfo GetLoginInfo()
    {
        LoginInfo loginInfo = new LoginInfo(Login, Password);

        return loginInfo;
    }
}