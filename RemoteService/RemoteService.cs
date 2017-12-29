using System;
using System.Diagnostics;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace RemoteService
{

    public sealed class RemoteService : IBackgroundTask
    {
        BackgroundTaskDeferral serviceDeferral;
        AppServiceConnection connection;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            try
            {
                serviceDeferral = taskInstance.GetDeferral(); // Get a deferral so that the service isn't terminated.
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

                var result = new ValueSet(); //key/value pair for the result, key is used on the client
                var deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
                result.Add("result", $"Hello from provider on {deviceInfo.FriendlyName}");
                var response = await args.Request.SendResponseAsync(result);
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
