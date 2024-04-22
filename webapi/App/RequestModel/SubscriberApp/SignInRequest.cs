namespace webapi.App.RequestModel.SubscriberApp
{
    public class SignInRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public bool Terminal { get; set; }

        //[Required]
        public string ApkVersion { get; set; }

        //[Required]
        public string CoordinateLocation { get; set; }
        //[Required]
        public string AddressLocation { get; set; }
    }
}