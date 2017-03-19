using System;
using System.ServiceProcess;

namespace Ulterius.Windows.Management.Core
{
    public static class WindowsServiceHelper
    {
        public const string UlteriusServiceName = "UlteriusServer";

        public static readonly TimeSpan ServiceTransitionTimeout = TimeSpan.FromSeconds(30);

        public static ServiceControllerStatus? Validate(string service)
        {
            ServiceController controller = null;
            try
            {
                controller = new ServiceController(service);
                var status = controller.Status; //This will throw an exception for an invalid service.
                return status;
            }
            catch (System.Exception exc)
            {
                //TODO: Log errors
                return null;
            }
            finally
            {
                controller?.Dispose();
            }
        }

        public static bool Start(string service)
        {
            ServiceController controller = null;
            try
            {
                controller = new ServiceController(service);

                if (controller.Status == ServiceControllerStatus.Running ||
                    controller.Status == ServiceControllerStatus.ContinuePending ||
                    controller.Status == ServiceControllerStatus.PausePending ||
                    controller.Status == ServiceControllerStatus.StartPending ||
                    controller.Status == ServiceControllerStatus.StopPending
                )
                {
                    return false;
                }
                controller.Start();

                controller.WaitForStatus(ServiceControllerStatus.Running, ServiceTransitionTimeout);
                return controller.Status == ServiceControllerStatus.Running;
            }
            catch (System.Exception exc)
            {
                //TODO: Log errors
                return false;
            }
            finally
            {
                controller?.Dispose();
            }
        }

        public static bool Stop(string service)
        {
            ServiceController controller = null;
            try
            {
                controller = new ServiceController(service);

                if (controller.Status == ServiceControllerStatus.Stopped ||
                    controller.Status == ServiceControllerStatus.ContinuePending ||
                    controller.Status == ServiceControllerStatus.PausePending ||
                    controller.Status == ServiceControllerStatus.StartPending ||
                    controller.Status == ServiceControllerStatus.StopPending
                )
                {
                    return false;
                }
                controller.Stop();

                controller.WaitForStatus(ServiceControllerStatus.Stopped, ServiceTransitionTimeout);
                return controller.Status == ServiceControllerStatus.Stopped;
            }
            catch (System.Exception exc)
            {
                //TODO: Log errors
                return false;
            }
            finally
            {
                controller?.Dispose();
            }
        }
    }
}