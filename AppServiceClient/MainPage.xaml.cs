using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.RemoteSystems;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        public MainPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

            AppServiceConnection connection = new AppServiceConnection()
            {
                AppServiceName = "com.poctools.remoteservice",                              //found in Package.appxmanifest of the provider on the Declarations tab
                PackageFamilyName = "5d38e6b4-600e-4644-a557-5a1e83ec29f4_73nr6b5p9v3r0"    //found in Package.appxmanifest of the provider on the Packaging tab
            };

            RemoteSystemConnectionRequest connectionRequest = new RemoteSystemConnectionRequest(remotesys);

            AppServiceConnectionStatus status = await connection.OpenRemoteAsync(connectionRequest);
            //AppServiceConnectionStatus status = await connection.OpenAsync();
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
                    return response.Message["result"] as string;
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
