﻿using Firebase.Storage;
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
        public async static void UploadFileWithAuth(string email, string password, string path, string displayName, string description)
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


                var task = new FirebaseStorage(
                     ConfigurationManager.AppSettings["appSpot"],
                     new FirebaseStorageOptions
                     {
                         AuthTokenAsyncFactory = () => Task.FromResult(result),
                         ThrowOnCancel = true,
                     })
                    .Child("Cloud_Saves")
                    .Child(Path.GetFileName(path))
                    .PutAsync(stream);

                // Track progress of the upload
                //task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                //Update database if file upload was succesful
                UpdateDatabase(email, password, true, path, displayName, description);
            }
            catch (Exception e)
            {
                Console.WriteLine("Login failed for " + email + " for file: " + path);
            }

            stream.Close();
        }

        public static async void UpdateDatabase(string email, string password, bool isPublic, string path, string displayName, string description)
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
                Path = "Cloud_Saves/" + Path.GetFileName(path),
                Public = isPublic,
                Username = email
            };
            var client = new FireSharp.FirebaseClient(firebaseConfig);
            var setter = client.Set("Cloud_Saves/" + Path.GetFileNameWithoutExtension(path), databaseEntry);
            
        
        }

        public static void CloudUpload(string email, string password)
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            List<string> displayNames = cache["displayName"] as List<string>;
            List<string> descriptions = cache["description"] as List<string>;
            if (filePaths != null && filePaths.Count >= 1)
            {
                for (int i = 0; i < filePaths.Count; i++)
                {
                    UploadFileWithAuth("z2omer@gmail.com", "z2omer", filePaths[i], displayNames[i], descriptions[i]);
                }
            }
        }
    }
}