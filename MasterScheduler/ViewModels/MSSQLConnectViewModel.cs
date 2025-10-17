using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MasterScheduler.ViewModels
{
    public partial class MSSQLConnectViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<string> servers;

        [ObservableProperty]
        private ObservableCollection<string> authentications = new ObservableCollection<string>
            {
                "Windows","SQL Server"
            };

        [ObservableProperty]
        private string selectedServer;

        [ObservableProperty]
        private string selectedAuthentication = "Windows";

        [ObservableProperty]
        private string loginID;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isRemember;

        [ObservableProperty]
        private bool shouldClose;

        [ObservableProperty]
        private bool isConnecting = false;

        [ObservableProperty]
        private bool isConnectedServer = false;

        public MSSQLConnectViewModel()
        {
            Servers = new ObservableCollection<string>
            {                
                "Browse..."
            };
        }
        partial void OnSelectedServerChanged(string value)
        {
            if (value == "Browse...")
            {
                BrowseServers();
            }
        }

        private void BrowseServers()
        {
            try
            {
                var server = SqlInstanceFinder.GetLocalSqlInstances();
                foreach (string servername in server)
                {                    

                    if (!Servers.Contains(servername))
                        Servers.Insert(0, servername);
                }
            }
            catch (Exception ex)
            {
                // In real app -> log it or show message
                System.Windows.MessageBox.Show("Error fetching SQL Servers: " + ex.Message);
            }
           
            if (!Servers.Contains("Browse...")) Servers.Add("Browse...");
        }

        [RelayCommand]
        public async Task Connect()
        {
            IsConnecting = true;
            isConnectedServer = true;
            await Task.Delay(5000);
            ShouldClose = true;
        }

        [RelayCommand]
        public void Cancel()
        {
            ShouldClose = true;
        }

       
    }
}
