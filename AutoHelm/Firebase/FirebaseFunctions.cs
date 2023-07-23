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

namespace AutoHelm.Firebase
{
    
    internal class FirebaseFunctions
    {
        public async static void UploadFileNoAuth(string path)
        {

            var stream = File.Open(path, FileMode.Open);

            // Construct FirebaseStorage with path to where you want to upload the file and put it there
            var task = new FirebaseStorage("autohelm.appspot.com")
             .Child("Cloud_Saves")
             .Child(Path.GetFileName(path))
             .PutAsync(stream);

            // Track progress of the upload
            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            // Await the task to wait until upload is completed and get the download url
            var downloadUrl = await task;

        }
        //Must ensure the user is a properly authenticated user before proceeding
        public async static void UploadFileWithAuth(string email, string password, string path)
        {
            var stream = File.Open(path, FileMode.Open);
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyAljhWFqObrRGHEIvq6_NEZW2sBd9Pml9g",
                AuthDomain = "autohelm.firebaseapp.com",
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


                var task = new FirebaseStorage(
                    "autohelm.appspot.com",
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(result),
                         ThrowOnCancel = true,
                     })
                    .Child("Cloud_Saves")
                    .Child(Path.GetFileName(path))
                    .PutAsync(stream);

                // Track progress of the upload
                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");
            }
            catch (Exception e)
            {
                Console.WriteLine("Login failed...");
            }
        }

        public static void UpdateDatabase(string email, string password, string path)
        {
            IFirebaseConfig firebaseConfig = new FirebaseConfig()
            {
                AuthSecret = "mnecF8JQt6DjYENkYdT36jxYG4plnLX1JEARrVjW",
                BasePath = "https://autohelm-default-rtdb.firebaseio.com/Cloud_Saves"
            };

            var databaseEntry = new
            {
                Created = "07/18/2023",
                Description = "Test",
                FileName = Path.GetFileName(path),
                Name = Path.GetFileNameWithoutExtension(path),
                Path = "Cloud_Saves/" + Path.GetFileName(path),
                Public = true,
                Username = email
            };
            var client = new FireSharp.FirebaseClient(firebaseConfig);
            var setter = client.Set("Cloud_Saves/" + Path.GetFileNameWithoutExtension(path), databaseEntry);
            Console.WriteLine("Hello");
            
        
        }
    }
}
