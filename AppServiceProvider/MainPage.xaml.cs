using System.Linq;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;


namespace AppServiceProvider
{

    public sealed partial class MainPage : Page
    {
        public string FamilyName { get; private set; }
        public string IpAddress { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            FamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;
            IpAddress = GetLocalIp();
        }

        private string GetLocalIp(HostNameType hostNameType = HostNameType.Ipv4)
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .FirstOrDefault(
                        hn =>
                            hn.Type == hostNameType &&
                            hn.IPInformation?.NetworkAdapter != null &&
                            hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);
            return hostname?.CanonicalName;
        }

    }
}
