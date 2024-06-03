using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OopLabProje.LoginForm;

namespace OopLabProje
{
    public partial class LoginForm : Form
    {
        //change the LoginForm class to a Singleton class
        public static LoginForm Instance { get; private set; }



        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public UserType UserType { get; set; }
        }


        public enum UserType
        {
            Admin,
            NormalUser,
            PartTimeUser
        }

        public static List<User> users; // In-memory user data
        public static User currentUser;

        public LoginForm()
        {
            Instance = this;

            InitializeComponent();
            CheckSaveDirectory();
            InitializeUsers();
        }

        private void InitializeUsers()
        {
            // Load user data from the file (or create an empty list)
            users = LoadUsersFromFile();
        }

        private List<User> LoadUsersFromFile()
        {
            try
            {
                
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "users.txt"); // !proje adi degisirse degistir
                if (File.Exists(filePath))
                {
                    // Read user data from the file (CSV format assumed)
                    return File.ReadAllLines(filePath)
                        .Select(line => line.Split(','))
                        .Select(parts => new User
                        {
                            Username = parts[0],
                            Password = parts[1],
                            UserType = (UserType)Enum.Parse(typeof(UserType), parts[2])
                        })
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Return an empty list if file doesn't exist or an error occurs
            return new List<User>();
        }

        public void SaveUsersToFile()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "users.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save user data to the file (CSV format)
                File.WriteAllLines(filePath, users.Select(u => $"{u.Username},{u.Password},{u.UserType}"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (AuthenticateUser(username, password))
            {
                // Successful login
                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Open the main application window or perform other actions

                currentUser = users.FirstOrDefault(u => u.Username == username);

                if (currentUser.UserType == UserType.Admin)
                {
                    // Admin login
                    // Provide access to admin-specific features

                    MainForm mainForm = new MainForm();
                    mainForm.Show();

                    this.Hide();
                }
                else
                {
                    // Normal user login
                    // Show regular user interface

                    MainForm mainForm = new MainForm();
                    mainForm.Show();
                }

            }
            else
            {
                // Invalid credentials
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private bool AuthenticateUser(string username, string password)
        {
            var user = users.FirstOrDefault(u => u.Username == username);
            if (user != null && user.Password == password)
            {
                // Successful authentication
                return true;
            }
            else
            {
                // Invalid credentials
                return false;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string newUsername = txtNewUsername.Text.Trim();
            string newPassword = txtNewPassword.Text;

            if (string.IsNullOrEmpty(newUsername) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Please enter a valid username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (users.Any(u => u.Username == newUsername))
            {
                MessageBox.Show("Username already exists. Choose a different one.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(newPassword != txtNewPasswordVerify.Text)
            {
                MessageBox.Show("Passwords do not match. Please verify the password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create a new user and add to the list
            var newUser = new User
            {
                Username = newUsername,
                Password = newPassword,
                UserType = users.Count == 0 ? UserType.Admin : UserType.NormalUser // First user is admin
            };
            users.Add(newUser);

            // Save the updated user list to the file
            SaveUsersToFile();

            MessageBox.Show("User registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void CheckSaveDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //if users.txt does not exist, create it
            string filePath = Path.Combine(path, "users.txt");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }
    }   
}

