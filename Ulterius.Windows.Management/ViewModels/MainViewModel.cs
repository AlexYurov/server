using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Ulterius.Windows.Management.Core;

namespace Ulterius.Windows.Management.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private bool _isTransitioning;
        public bool IsTransitioning
        {
            get { return _isTransitioning; }
            private set
            {
                _isTransitioning = value;
                RaisePropertyChanged();
                RefreshCommand.RaiseCanExecuteChanged();
                ToggleStateCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isServiceReady;
        public bool IsServiceReady
        {
            get { return _isServiceReady; }
            private set
            {
                _isServiceReady = value;
                RaisePropertyChanged();
                ToggleStateCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isServiceRunning;
        public bool IsServiceRunning
        {
            get { return _isServiceRunning; }
            private set
            {
                _isServiceRunning = value;
                RaisePropertyChanged();
            }
        }

        private string _log;
        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand ToggleStateCommand { get; }
        public RelayCommand RefreshCommand { get; }

        public MainViewModel()
        {
            ToggleStateCommand = new RelayCommand(ToggleStateCommandExecute, ToggleStateCommandCanExecute);
            RefreshCommand = new RelayCommand(RefreshCommandExecute, RefreshCommandCanExecute);
        }

        private bool RefreshCommandCanExecute()
        {
            return !IsTransitioning;
        }

        private async void RefreshCommandExecute()
        {
            if (IsTransitioning) return;
            IsTransitioning = true;
            AppendLog("Refreshing service status...");
            var tcs = new TaskCompletionSource<ServiceControllerStatus?>();
            ThreadPool.QueueUserWorkItem(ValidateServiceInBackground, tcs);
            var result = await tcs.Task;
            IsServiceReady = result != null;
            AppendLog($"Service was {(IsServiceReady ? "": "not ")}found.");
            IsServiceRunning = result != ServiceControllerStatus.Stopped;
            if (IsServiceReady)
            {
                AppendLog($"Service is {(IsServiceRunning ? "running" : "stopped")}.");
            }
            IsTransitioning = false;
        }

        private void ValidateServiceInBackground(object state)
        {
            var tcs = (TaskCompletionSource<ServiceControllerStatus?>)state;
            Thread.Sleep(1500);
            var status = WindowsServiceHelper.Validate(WindowsServiceHelper.UlteriusServiceName);
            tcs.SetResult(status);
        }

        private bool ToggleStateCommandCanExecute()
        {
            return IsServiceReady && !_isTransitioning;
        }

        private async void ToggleStateCommandExecute()
        {
            if (!ToggleStateCommandCanExecute()) return;
            IsTransitioning = true;
            AppendLog($"{(IsServiceRunning ? "Stopping" : "Starting")} the service...");
            var tcs = new TaskCompletionSource<bool>();
            ThreadPool.QueueUserWorkItem(ToggleServiceStateInBackground, tcs);
            var result = await tcs.Task;
            if (result)
            {
                IsServiceRunning = !IsServiceRunning;
                AppendLog($"The service was {(IsServiceRunning ? "started" : "stopped")} successfully.");
            }
            else
            {
                AppendLog($"Failed to update the service state.");
            }
            IsTransitioning = false;
        }

        private void ToggleServiceStateInBackground(object state)
        {
            var tcs = (TaskCompletionSource<bool>)state;
            Thread.Sleep(1500);
            var result = IsServiceRunning 
                ? WindowsServiceHelper.Stop(WindowsServiceHelper.UlteriusServiceName) 
                : WindowsServiceHelper.Start(WindowsServiceHelper.UlteriusServiceName);
            tcs.SetResult(result);
        }

        private void AppendLog(string line)
        {
            Log += $"[{DateTime.Now:HH:mm:ss.ffff}]: {line}{Environment.NewLine}";
        }
    }
}