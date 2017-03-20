namespace UlteriusServer.Api.Network.Models
{
    public class ServiceInformation
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string StartType { get; set; }
        public string Status { get; set; }
        public string DisplayName { get; set; }
        public bool CanStop { get; set; }
        public int ServiceType { get; set; }
        public bool CanPauseAndContinue { get; set; }
    }
}