using System;
using System.Windows;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Configuration;

namespace AutoHelm.pages
{
    public partial class LoginPopUp : Window
    {
        public LoginPopUp()
        {
            InitializeComponent();
        }

        private async void tryLogin(string email, string password)
        {
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

                MessageBox.Show("Login successful!");
            }
            catch (Exception e)
            {
                MessageBox.Show("Login failed: " + e);
            }
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
//            string email = txtEmail.Text;
//            string password = txtPassword.Password;
 //           tryLogin(email, password);
        }
    }
}
