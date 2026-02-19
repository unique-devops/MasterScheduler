using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public class GoogleDriveHelper
    {
        private readonly string _appName = "MasterScheduler";

        public async Task<bool> IsAuthorizedAsync(string email)
        {
            var dataStore = new MySqliteDataStore();

            var token = await dataStore.GetAsync<TokenResponse>(email);
            return (token != null);            
        }

        public async Task<UserCredential> AuthorizeTempAsync()
        {
            var assembly = typeof(GoogleDriveHelper).Assembly;
            string resourceName = "MasterScheduler.Shared.credentials.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {                               
                // This call will now be 100% silent because the token exists in the dataStore
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile, "https://www.googleapis.com/auth/userinfo.email" },
                    "temp",
                    CancellationToken.None,
                    new NullDataStore());
            }
        }

        public async Task<(string Email, string GoogleUserId)> GetLoginInfoAsync(UserCredential credential)
        {
            var oauth2 = new Oauth2Service(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var user = await oauth2.Userinfo.Get().ExecuteAsync();
            return (user.Email, user.Id);
        }

        public async Task SaveAuthAsync(string email, UserCredential credential)
        {
            var dataStore = new MySqliteDataStore();

            await dataStore.StoreAsync(email, credential.Token);
        }
               
        public async Task<UserCredential> GetAccountCredentialsAsync(string Email)
        {
            var assembly = typeof(GoogleDriveHelper).Assembly;
            string resourceName = "MasterScheduler.Shared.credentials.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var dataStore = new MySqliteDataStore();

                // IMPORTANT: Check if the token exists for the "user" key in SQLite first
                var existingToken = await dataStore.GetAsync<TokenResponse>(Email);

                if (existingToken == null)
                {
                    // If no token, do NOT call AuthorizeAsync (it would try to open browser)
                    throw new Exception("No Google Drive token found in database. Please authorize via the UI first.");
                }

                // This call will now be 100% silent because the token exists in the dataStore
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile, "https://www.googleapis.com/auth/userinfo.email" },
                    Email,
                    CancellationToken.None,
                    dataStore);
            }

        }

        public async Task<string> GetOrCreateFolderAsync(UserCredential credential, string folderName)
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName               
            });

            // 1. SEARCH: Check if a folder with this name already exists
            var listRequest = service.Files.List();
            listRequest.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
            listRequest.Fields = "files(id, name)";

            var searchResult = await listRequest.ExecuteAsync();
            var existingFolder = searchResult.Files.FirstOrDefault();

            if (existingFolder != null)
            {
                // Folder exists! Return the existing ID
                return existingFolder.Id;
            }

            // 2. CREATE: If not found, create a new one
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var createRequest = service.Files.Create(folderMetadata);
            createRequest.Fields = "id";

            var newFolder = await createRequest.ExecuteAsync();
            return newFolder.Id;
        }
        
        public async Task<GoogleDriveConfig> GetAccountDetailsAndFolders(UserCredential credential)
        {
            // 1. Create the Drive Service
            var driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName
            });

            // 2. GET EMAIL: Use the 'About' service
            var aboutRequest = driveService.About.Get();
            aboutRequest.Fields = "user";
            var about = await aboutRequest.ExecuteAsync();
            string userEmail = about.User.EmailAddress;

            // 3. GET FOLDERS: List all folders so the user can pick one in WPF
            var listRequest = driveService.Files.List();
            listRequest.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            listRequest.Fields = "files(id, name)";
            var folderList = await listRequest.ExecuteAsync();

            // Now you have everything!
            return new GoogleDriveConfig
            {
                UserEmail = userEmail,
                FolderList = folderList.Files.ToList() // Pass this to your WPF ComboBox
            };
        }
        
        public async Task<(bool success, string email)> TestAuthOnlyAsync(UserCredential credential)
        {
            try
            {
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _appName
                });

                // This only fetches the user's basic account info
                var request = service.About.Get();
                request.Fields = "user(emailAddress)";

                var about = await request.ExecuteAsync();

                // If we get here, the connection is alive!
                return (true, about.User.EmailAddress);
            }
            catch (TokenResponseException)
            {
                // Token is expired/revoked and cannot be refreshed
                return (false, "Auth Expired");
            }
            catch (Exception)
            {
                // General error (No internet, etc.)
                return (false, "Connection Failed");
            }
        }
        public async Task<(bool success, string message)> TestConnectionAsync(UserCredential credential, string folderId)
        {
            try
            {
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _appName
                });

                // Try to get only the metadata for the specific folder
                var request = service.Files.Get(folderId);
                request.Fields = "id, name, trashed";

                var folder = await request.ExecuteAsync();

                if (folder.Trashed == true)
                {
                    return (false, "Folder exists but is in the Trash.");
                }

                return (true, $"Connected! Target: {folder.Name}");
            }
            catch (TokenResponseException)
            {
                return (false, "Session expired. Please Re-Authorize.");
            }
            catch (GoogleApiException ex) when (ex.Error.Code == 404)
            {
                return (false, "Target folder not found on Google Drive.");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
       
        public async Task UploadBackup(UserCredential credential,string fileName,string filePath,GoogleDriveConfig driveConfig, CancellationToken ct)
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName,
            });

            await service.Files.Get(driveConfig.TargetFolderId).ExecuteAsync(ct);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { driveConfig.TargetFolderId } // Optional: target folder ID
            };

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // "application/octet-stream" is safer for general files, 
                // but "application/zip" is fine if you are strictly uploading zips.
                var request = service.Files.Create(fileMetadata, stream, "application/zip");
                request.Fields = "id";

                var progress = await request.UploadAsync(ct);

                if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                {
                    throw new Exception($"Upload failed: {progress.Exception.Message}");
                }
            }
        }

        public async Task<string> CleanOldBackupsAsync(UserCredential credential, string targetFolderId, int retentionDays,CancellationToken token)
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName,
            });

            // 1. Calculate the cutoff date
            DateTime cutoffDate = DateTime.Now.AddDays(-retentionDays);
            // Google Drive uses RFC 3339 format (yyyy-MM-ddTHH:mm:ssZ)
            string rfcCutoff = cutoffDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // 2. Search for files in that folder older than the cutoff
            var listRequest = service.Files.List();
            listRequest.Q = $"'{targetFolderId}' in parents and createdTime < '{rfcCutoff}' and trashed = false";
            listRequest.Fields = "files(id, name, createdTime)";

            var filesToDelete = await listRequest.ExecuteAsync(token);

            // 3. Loop and Delete
            if (filesToDelete.Files != null && filesToDelete.Files.Count > 0)
            {
                foreach (var file in filesToDelete.Files)
                {
                    try
                    {
                        // Permanent delete: service.Files.Delete(file.Id).Execute();
                        // Safer "Move to Trash":
                        var updateFile = new Google.Apis.Drive.v3.Data.File { Trashed = true };
                        await service.Files.Update(updateFile, file.Id).ExecuteAsync(token);

                        //Console.WriteLine($"Deleted old backup: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
            }

            return "success";
        }

        public async Task<UserCredential> GetCredentialsAsync()
        {
            var assembly = typeof(GoogleDriveHelper).Assembly;
            // Check the exact path (Namespace.FileName.json)
            string resourceName = "MasterScheduler.Shared.credentials.json";

            using (Stream stream = assembly?.GetManifestResourceStream(resourceName))
            {
                if (stream == null) throw new Exception("Credential resource not found!");

                // Use your SQLite-backed DataStore instead of FileDataStore
                var dataStore = new MySqliteDataStore();

                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile, "https://www.googleapis.com/auth/userinfo.email" },
                    "user",
                    CancellationToken.None,
                    dataStore);
            }
        }

        public async Task<UserCredential> GetSilentCredentialsAsync()
        {
            var assembly = typeof(GoogleDriveHelper).Assembly;
            string resourceName = "MasterScheduler.Shared.credentials.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var dataStore = new MySqliteDataStore();

                // IMPORTANT: Check if the token exists for the "user" key in SQLite first
                var existingToken = await dataStore.GetAsync<TokenResponse>("user");

                if (existingToken == null)
                {
                    // If no token, do NOT call AuthorizeAsync (it would try to open browser)
                    throw new Exception("No Google Drive token found in database. Please authorize via the UI first.");
                }

                // This call will now be 100% silent because the token exists in the dataStore
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveFile, "https://www.googleapis.com/auth/userinfo.email" },
                    "user",
                    CancellationToken.None,
                    dataStore);
            }
        }
        public async Task<IList<Google.Apis.Drive.v3.Data.File>> GetDriveFoldersAsync(UserCredential credential)
        {
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "MasterScheduler"
            });

            var request = service.Files.List();
            request.Q = "mimeType = 'application/vnd.google-apps.folder' and trashed = false";
            request.Fields = "files(id, name)";
            request.Spaces = "drive";

            var result = await request.ExecuteAsync();
            return result.Files;
        }

    }
}
