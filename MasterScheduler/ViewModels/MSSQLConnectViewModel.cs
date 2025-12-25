using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Helper;
using Microsoft.Data.SqlClient;
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
        private ObservableCollection<string> servers = new ObservableCollection<string>
        {
            ".","Browse..."
        };

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
        private string loginID="";

        [ObservableProperty]
        private string password ="";

        [ObservableProperty]
        private bool isRemember;

        [ObservableProperty]
        private bool shouldClose;

        [ObservableProperty]
        private bool isConnecting = false;

        [ObservableProperty]
        private bool isConnectedServer = false;
        
        public string ConnectedString = "";

        public MSSQLConnectViewModel()
        {
            
        }
        public void SetModelData()
        {
            if (!Servers.Contains(SelectedServer)) Servers.Insert(0, SelectedServer);
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
                var server = SqlInstanceFinder.GetAllLocalSqlInstances();
                server.Insert(0, ".");
                Servers = new ObservableCollection<string>(server);
                //foreach (string servername in server)
                //{                    

                //    if (!Servers.Contains(servername))
                //        Servers.Insert(0, servername);
                //}
            }
            catch (Exception ex)
            {
                // In real app -> log it or show message
                System.Windows.MessageBox.Show("Error fetching SQL Servers: " + ex.Message);
            }
           
            if (!Servers.Contains("Browse...")) Servers.Insert(Servers.Count,"Browse...");
            SelectedServer = Servers?.FirstOrDefault() ?? ".";
        }

        [RelayCommand]
        public async Task Connect()
        {
            IsConnecting = true;
            var result = await TestAdvancedConnectionAsync(SelectedServer);
            IsConnectedServer = result.Success;            
            ShouldClose = result.Success;
            IsConnecting = false;
        }

        [RelayCommand]
        public void Cancel()
        {
            ShouldClose = true;
        }


        public async Task<(bool Success, string Message)> TestAdvancedConnectionAsync(string serverName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = "master",
                ConnectTimeout = 5,
                // Required for SQL Server 2022+ compatibility
                TrustServerCertificate = true
            };

            if (SelectedAuthentication.ToLower() == "windows")
            {
                // Use Windows Authentication
                builder.IntegratedSecurity = true;
            }
            else
            {
                // Use SQL Server Authentication
                builder.IntegratedSecurity = false;
                builder.UserID = LoginID;
                builder.Password = Password;
            }

            try
            {
                ConnectedString = builder.ConnectionString;
                using (SqlConnection connection = new SqlConnection(ConnectedString))
                {
                    await connection.OpenAsync();
                    return (true, "Success!");
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return (false, ex.Message);
            }
        }
    }
}
