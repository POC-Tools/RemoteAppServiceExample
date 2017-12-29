using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace RemoteService
{
    public sealed class RemoteService:IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                serviceDeferral = taskInstance.GetDeferral();
                taskInstance.Canceled += OnTaskCanceled;
                var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                if (details.Name == "com.poctools.remoteservice")
                {
                    connection = details.AppServiceConnection;
                    connection.RequestReceived += OnRequestReceived;
                    connection.ServiceClosed += Connection_ServiceClosed;
                }
                else
                {
                    serviceDeferral.Complete();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }


        async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();
            try
            {
                var input = args.Request.Message;

                var result = new ValueSet();
                //var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
                //var deviceName = deviceInfo.FriendlyName;
                result.Add("result", $"Hello from provider");
                //Debug.WriteLine(json);
                var response = await args.Request.SendResponseAsync(result); //returns Success, even on the phone-to-desktop scenario
                Debug.WriteLine($"Send result: {response}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                messageDeferral.Complete();
            }
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine($"Service closed: {args.Status}");
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine($"Task cancelled: {reason}");
                  
            if (serviceDeferral != null)
            {
                serviceDeferral.Complete();
                serviceDeferral = null;
            }

            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }


    }
}
