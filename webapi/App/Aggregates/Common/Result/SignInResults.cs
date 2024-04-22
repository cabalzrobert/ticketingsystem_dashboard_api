namespace webapi.App.Aggregates.Common
{
    public enum SignInResults
    {
        Null,
        Failed,
        Success,
        Expired,
        Blocked,
        PreRegister,
        ChangePassword,
        OTPFailed,
        ApkUpdate,
    }
}