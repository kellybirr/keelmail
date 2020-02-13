using System;

namespace Keelmail.Models
{
    public class SmtpConfig
    {
        public Uri ServerUri { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }

        internal bool IsValid()
        {
            return (
                    !string.IsNullOrWhiteSpace(FromAddress) &&
                    !string.IsNullOrWhiteSpace(ToAddress) &&
                    !string.IsNullOrWhiteSpace(ServerUri.Host) &&
                    ServerUri.Port != 0 &&
                    (string.IsNullOrEmpty(User) || !string.IsNullOrEmpty(Password))
            );
        }
    }
}
