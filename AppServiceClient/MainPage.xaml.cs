using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AppServiceClient
{

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public string RemoteIpAddress { get; set; } = "192.168.1.2";
        string response;
        public string Response
        {
            get { return response; }
            set { response = value; NotifyPropertyChanged(); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void requestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(RemoteIpAddress))
            {
                RemoteSystemAccessStatus status = await RemoteSystem.RequestAccessAsync();
                var sys = await GetDeviceByAddressAsync(RemoteIpAddress);
                Response = await OpenRemoteConnectionAsync(sys);
            }
        }

        private async Task<string> OpenRemoteConnectionAsync(RemoteSystem remotesys)
        {
            if (remotesys == null)
                return "No remote system";
            //Create a connection for the service
            AppServiceConnection connection = new AppServiceConnection()
            {
                AppServiceName = "com.poctools.remoteservice",                              //found in Package.appxmanifest of the provider on the Declarations tab
                PackageFamilyName = "5d38e6b4-600e-4644-a557-5a1e83ec29f4_73nr6b5p9v3r0"    //found in Package.appxmanifest of the provider on the Packaging tab
            };
            //Connect to the remote system
            RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest(remotesys);
            //open a connection to the service on the remote system
            AppServiceConnectionStatus status = await connection.OpenRemoteAsync(connectionRequest);
            //AppServiceConnectionStatus status = await connection.OpenAsync(); //use OpenAsync() if service is on the same device
            if (status != AppServiceConnectionStatus.Success)
            {
                return status.ToString();
            }
            ValueSet inputs = new ValueSet();

            try
            {
                AppServiceResponse response = await connection.SendMessageAsync(inputs);
                // check that the service successfully received and processed the message
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    // Get the data that the service returned:
                    return response.Message["result"] as string; //"result" is the key used in RemoteService.OnRequestReceived() ValueSet
                }
                return response.Status.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        private async Task<RemoteSystem> GetDeviceByAddressAsync(string IPaddress)
        {
            // construct a HostName object
            Windows.Networking.HostName deviceHost = new Windows.Networking.HostName(IPaddress);
            // create a RemoteSystem object with the HostName
            RemoteSystem remotesys = await RemoteSystem.FindByHostNameAsync(deviceHost);
            return remotesys;
        }
    }
}
