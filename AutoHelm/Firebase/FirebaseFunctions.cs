using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Storage;
using Firebase.Auth;
using Firebase.Auth.Providers;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using System.ComponentModel;
using System.Runtime.Caching;
using System.Configuration;

namespace AutoHelm.Firebase
{

    internal class FirebaseFunctions
    {
        public static volatile int isSaving = 0;
        public async static void UploadFileNoAuth(string path)
        {

            var stream = File.Open(path, FileMode.Open);

            // Construct FirebaseStorage with path to where you want to upload the file and put it there
            var task = new FirebaseStorage(ConfigurationManager.AppSettings["appSpot"])
             .Child("Cloud_Saves")
             .Child(Path.GetFileName(path))
             .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            // Await the task to wait until upload is completed and get the download url
            var downloadUrl = await task;

            stream.Close();

        }
        //Must ensure the user is a properly authenticated user before proceeding
        public async static Task<bool> UploadFileWithAuth(string email, string password, string path, string displayName, string description, bool isPrivate)
        {
            var stream = File.Open(path, FileMode.Open);
            var config = new FirebaseAuthConfig
            {
                ApiKey = ConfigurationManager.AppSettings["apiKey"],
                AuthDomain = ConfigurationManager.AppSettings["domain"],
                Providers = new FirebaseAuthProvider[]
                {
                    new GoogleProvider(),
                    new FacebookProvider(),
                    new TwitterProvider(),
                    new GithubProvider(),
                    new MicrosoftProvider(),
                    new EmailProvider()
                }
            };
            try
            {

                var client = new FirebaseAuthClient(config);
                UserCredential uc = await client.SignInWithEmailAndPasswordAsync(email, password);
                var result = await uc.User.GetIdTokenAsync();


                //if project was declared private, we put it in the private directory in firebase
                if (isPrivate)
                {
                    var task = new FirebaseStorage(
                     ConfigurationManager.AppSettings["appSpot"],
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(result),
                         ThrowOnCancel = true,
                     })
                    .Child("Private")
                    .Child(email)
                    .Child(Path.GetFileName(path))
                    .PutAsync(stream);
                }
                //otherwise put it in the public directory where all public workflows exist
                else
                {
                    var task = new FirebaseStorage(
                     ConfigurationManager.AppSettings["appSpot"],
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(result),
                         ThrowOnCancel = true,
                     })
                    .Child("Public")
                    .Child(Path.GetFileName(path))
                    .PutAsync(stream);
                }

                // Track progress of the upload
                //task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                //Update database if file upload was succesful
                UpdateDatabase(email, password, path, displayName, description, isPrivate);
            }
            catch (Exception e)
            {
                Console.WriteLine("Login failed for " + email + " for file: " + path);
                return false;
            }

            stream.Close();
            return true;
        }

        public static async void UpdateDatabase(string email, string password, string path, string displayName, string description, bool isPrivate)
        {
            IFirebaseConfig firebaseConfig = new FirebaseConfig()
            {
                AuthSecret = ConfigurationManager.AppSettings["secret"],
                BasePath = ConfigurationManager.AppSettings["basePath"],
            };

            var databaseEntry = new
            {
                Created = (DateTime.Now.ToString("dd/MM/yyyy")),
                Description = description,
                FileName = Path.GetFileName(path),
                Name = displayName,
                //The path will defer depending on if the workflow is public or private
                Path = isPrivate ? "Private/" + email + "/" + Path.GetFileName(path) : "Public/" + Path.GetFileName(path),
                Public = isPrivate,
                Username = email
            };

            var client = new FireSharp.FirebaseClient(firebaseConfig);

            //if the workflow is private, the database entry should be in the Private Section otherwise it will be in the Public Section
            if (isPrivate)
            {
                var parsedEmail = email.Split("@");
                var userId = parsedEmail[0] + parsedEmail[1].Split(".")[0];
                var setter = client.Set("Private/"+ userId + "/" + Path.GetFileNameWithoutExtension(path), databaseEntry);
            }
            else
            {
                var setter = client.Set("Public/" + Path.GetFileNameWithoutExtension(path), databaseEntry);
            }

        }

        public static bool CloudUpload(string email, string password)
        {
            bool ret = false;
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            List<string> displayNames = cache["displayName"] as List<string>;
            List<string> descriptions = cache["description"] as List<string>;
            List<bool> isPrivateStatus = cache["isPrivate"] as List<bool>;

            if (filePaths != null && filePaths.Count >= 1)
            {
                ret = true;

                for (int i = 0; i < filePaths.Count; i++)
                {
                    Task<bool> task = UploadFileWithAuth("z2omer@gmail.com", "z2omer", filePaths[i], displayNames[i], descriptions[i], isPrivateStatus[i]);
                    bool result = task.Result;

                    if (!result)
                    {
                        ret = false;
                    }
                }
            }

            return ret;
        }
    }
}
