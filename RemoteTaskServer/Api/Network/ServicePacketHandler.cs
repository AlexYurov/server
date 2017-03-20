using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using UlteriusServer.Api.Network.Messages;
using UlteriusServer.Api.Network.Models;
using UlteriusServer.WebSocketAPI.Authentication;
using vtortola.WebSockets;

namespace UlteriusServer.Api.Network
{
    public class ServicePacketHandler : PacketHandler
    {
        public static readonly TimeSpan ServiceTransitionTimeout = TimeSpan.FromSeconds(30);

        private AuthClient _authClient;
        private MessageBuilder _builder;
        private WebSocket _client;
        private Packet _packet;

        private void RequestServiceInformation()
        {
            var services = GetServices().ToList();
            _builder.WriteMessage(services);
        }

        private IEnumerable<ServiceInformation> GetServices()
        {
            foreach (var service in ServiceController.GetServices())
            {
                yield return new ServiceInformation
                {
                    ServiceName = service.ServiceName,
                    DisplayName = service.DisplayName,
                    StartType = service.StartType.ToString(),
                    CanStop = service.CanStop,
                    CanPauseAndContinue = service.CanPauseAndContinue,
                    ServiceType = (int)service.ServiceType,
                    Status = service.Status.ToString()

                };
            }
        }

        private void StartService()
        {
            //TODO: Exception handling
            var serviceName = _packet.Args[0].ToString();
            var result = Start(serviceName);
            var report = new
            {
                ServiceName = serviceName,
                Operation = "Start",
                IsSuccess = result
            };
            _builder.WriteMessage(report);
        }

        private void StopService()
        {
            //TODO: Exception handling
            var serviceName = _packet.Args[0].ToString();
            var result = Stop(serviceName);
            var report = new
            {
                ServiceName = serviceName,
                Operation = "Stop",
                IsSuccess = result
            };
            _builder.WriteMessage(report);
        }

        private static bool Start(string service)
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

        private static bool Stop(string service)
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

        public override void HandlePacket(Packet packet)
        {
            _client = packet.Client;
            _authClient = packet.AuthClient;
            _packet = packet;
            _builder = new MessageBuilder(_authClient, _client, _packet.EndPointName, _packet.SyncKey);
            switch (_packet.EndPoint)
            {
                case PacketManager.EndPoints.RequestServiceInformation:
                    RequestServiceInformation();
                    break;
                case PacketManager.EndPoints.StartService:
                    StartService();
                    break;
                case PacketManager.EndPoints.StopService:
                    StopService();
                    break;
            }
        }
    }
}