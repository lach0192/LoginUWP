/*
 *      FILE:           MainPage.xaml.cs
 *      PROGRAMMER:     Eric Lachapelle
 * 
 *      INFO:           Simple UWP Login app with random background image.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LoginUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string username;
        private string password;
        private byte numberOfLoginAttempts = 0;
        private bool accountLocked = false;
        private bool loginSuccess = false;
        private bool welcomeReached = false;
        private const byte MAX_LOGIN_ATTEMPTS = 3;

        private enum OutputMsg : int {Account_Locked, Welcome_Back, Invalid_Login_Attempt};

        public MainPage()
        {
            this.InitializeComponent();

            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = "Eric Lachapelle's Login App";

            this.username = "Eric";
            this.password = "password";
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (accountLocked == false)
            {
                // increase login attempt counter
                numberOfLoginAttempts++;

                // get values from form
                string enteredUsername = usernameTxt.Text;
                string enteredPassword = passwordTxt.Password;

                if (numberOfLoginAttempts <= MAX_LOGIN_ATTEMPTS)
                {
                    // successful login
                    if (enteredUsername == username && enteredPassword == password)
                    {
                        // welcome back
                        OutputMessage(OutputMsg.Welcome_Back);

                        // reset login attempt counter
                        numberOfLoginAttempts = 0;

                        // set access to true
                        loginSuccess = true;
                    }
                    else
                    {
                        // invalid login attempt
                        OutputMessage(OutputMsg.Invalid_Login_Attempt);

                        // set access to false
                        loginSuccess = false;
                    }
                }
                else
                {
                    // account is now locked
                    accountLocked = true;
                    OutputMessage(OutputMsg.Account_Locked);

                    // change image source
                    loginImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/locked.png")); ;

                    // disable txt input
                    usernameTxt.IsEnabled = false;
                    passwordTxt.IsEnabled = false;
                }
            }
        }

        private async void Welcome()
        {
            welcomeReached = true;

            // create an instance of BingImageOfTheDay
            BingImageOfTheDay dailyImg = new BingImageOfTheDay();
            Uri imgUri = await dailyImg.GetImage();

            loginGrid.Background = new ImageBrush { ImageSource = new BitmapImage(new Uri(this.BaseUri, imgUri)), Stretch = Stretch.Fill };

            // hide controls
            loginLabelTxt.Visibility = Visibility.Collapsed;
            usernameTxt.Visibility = Visibility.Collapsed;
            passwordTxt.Visibility = Visibility.Collapsed;
            loginBtn.Visibility = Visibility.Collapsed;
            loginImg.Visibility = Visibility.Collapsed;

            // create textbox
            TextBox imgInfoTxt = new TextBox();
            imgInfoTxt.VerticalAlignment = VerticalAlignment.Bottom;
            imgInfoTxt.HorizontalAlignment = HorizontalAlignment.Center;
            imgInfoTxt.FontSize = 16;
            imgInfoTxt.Height = 20;
            imgInfoTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
            imgInfoTxt.IsReadOnly = true;

            // set txt to img description
            imgInfoTxt.Text = dailyImg.GetDescription();

            // add textbox to grid
            loginGrid.Children.Add(imgInfoTxt);

            // speak info
            var mediaElement = loginMediaElement;
            var synth = new SpeechSynthesizer();
            var stream = await synth.SynthesizeTextToStreamAsync(dailyImg.GetDescription());
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private void OutputMessage(OutputMsg msg)
        {
            switch (msg)
            {
                case OutputMsg.Account_Locked:

                    // tell user account is locked
                    loginLabelTxt.Text = msg.ToString().Replace("_", " ");
                    loginLabelTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
                    break;

                case OutputMsg.Welcome_Back:

                    // greet user with welcome msg
                    loginLabelTxt.Text = msg.ToString().Replace("_", " ") + " " + username + "!";
                    loginLabelTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 224));
                    break;

                case OutputMsg.Invalid_Login_Attempt:

                    // give error msg to user
                    loginLabelTxt.Text = msg.ToString().Replace("_", " ");
                    loginLabelTxt.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 153, 0, 204));
                    break;
            }

            Speak();
        }

        private async void Speak()
        {
            var mediaElement = loginMediaElement;

            var synth = new SpeechSynthesizer();

            var msgToSpeak = loginLabelTxt.Text;

            var stream = await synth.SynthesizeTextToStreamAsync(msgToSpeak);

            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        private void UsernameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (accountLocked == false)
            {
                usernameTxt.Text = "";
            }
        }

        private void PasswordTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordTxt.Password = "";
        }

        private void LoginGrid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                // on enter click
                case Windows.System.VirtualKey.Enter:
                    LoginBtn_Click(sender, e);
                    break;
            }
        }

        private void LoginMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // when the speechsynth is done playing..
            if (loginSuccess == true)
            {
                if (welcomeReached == false)
                {
                    Welcome();
                }
            }
        }
    }
}