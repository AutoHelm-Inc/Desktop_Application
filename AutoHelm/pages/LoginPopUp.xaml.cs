using System;
using System.Windows;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Configuration;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutoHelm.pages
{
    public partial class LoginPopUp : Page
    {
        private readonly string emailPlaceholder = "Email";
        private readonly string passwordPlaceholder = "Password";
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
                NavigationService.Navigate(new HomePage());

            }
            catch (Exception e)
            {
                //TODO show failed message
                MessageSpace.Text = "Login failed";
            }
        }
        private async void tryReg(string email, string password)
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
                UserCredential uc = await client.CreateUserWithEmailAndPasswordAsync(email, password);

                MessageSpace.Text = "Registration successful!";
            }
            catch (FirebaseAuthException e)
            {
                MessageSpace.Text = "Registration failed";
            }
            catch (Exception e)
            {
                MessageSpace.Text = "Registration failed";
            }
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            MessageSpace.Text = "";
            string email = txtUser.Text;
            string password = txtPass.Password;
            AutoHelm.pages.MainWindow.usernameTopLevel.email = email;
            tryLogin(email, password);
        }
        private void btnRegis_Click(object sender, RoutedEventArgs e)
        {
            MessageSpace.Text = "";
            string email = txtUser.Text;
            string password = txtPass.Password;
            tryReg(email, password);
        }

        private void btnGuest_Click(object sender, RoutedEventArgs e)
        {
            MessageSpace.Text = "";
            NavigationService.Navigate(new HomePage());
        }
    }
}