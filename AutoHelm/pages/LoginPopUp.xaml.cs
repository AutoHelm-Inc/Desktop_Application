using System;
using System.Windows;
using Firebase.Auth;
using Firebase.Auth.Providers;
using System.Configuration;
using System.Windows.Controls;

namespace AutoHelm.pages
{
    public partial class LoginPopUp : Page
    {
        private readonly string emailPlaceholder = "Email";
        private readonly string passwordPlaceholder = "Password";
        public LoginPopUp()
        {
            InitializeComponent();
//            SetPlaceholderText();
        }
        /*        private void SetPlaceholderText()
                {
                    txtEmail.Text = emailPlaceholder;
                    txtPassword.Password = passwordPlaceholder;
                }
        */
        /*        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
                {
                    if (txtEmail.Text == emailPlaceholder)
                    {
                        txtEmail.Text = string.Empty;
                    }
                }
                private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
                {
                    if (txtPassword.Password == passwordPlaceholder)
                    {
                        txtPassword.Password = string.Empty;
                    }
                }*/
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

                NavigationService.Navigate(new HomePage());
            }
            catch (Exception e)
            {
                MessageBox.Show("Login failed for " + email);
                //                MessageBox.Show("Login failed: " + e);
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
                
                MessageBox.Show("Registration successful!");
            }
            catch (FirebaseAuthException e)
            {
                // Registration failed, show an error message
                MessageBox.Show($"Registration failed: {e.Reason}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //                MessageBox.Show("Login failed: " + e);
            }
        }
        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            txtEmail.Text = "";
            txtPassword.Password = "";
            tryLogin(email, password);
        }
        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            txtEmail.Text = "";
            txtPassword.Password = "";
            tryReg(email, password);
        }
    }
}
