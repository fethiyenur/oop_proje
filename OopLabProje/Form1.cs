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
using System.Text.RegularExpressions;
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

            public PersonalInformation PersonalInfo { get; set; }

        }
        public class PersonalInformation
        {
            public string Username { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }

            //Profile picture as base64 image string
            public string ProfilePicture { get; set; }

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

                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "users.csv"); // !proje adi degisirse degistir
                if (File.Exists(filePath))
                {
                    //Find the PersonalInformations from file then assign them to the users
                    string filePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "PersonalInformation.csv");
                    if (File.Exists(filePath2))
                    {
                        var personalInfos = File.ReadAllLines(filePath2)
                            .Select(line => line.Split(','))
                            .Select(parts => new PersonalInformation
                            {
                                Username = parts[0],
                                Name = parts[1],
                                Surname = parts[2],
                                Email = parts[3],
                                Phone = parts[4],
                                Address = parts[5],

                            })
                            .ToList();

                        return File.ReadAllLines(filePath)
                            .Select(line => line.Split(','))
                            .Select(parts => new User
                            {
                                Username = parts[0],
                                Password = parts[1],
                                UserType = (UserType)Enum.Parse(typeof(UserType), parts[2]),
                                PersonalInfo = personalInfos.FirstOrDefault(p => p.Username == parts[0])
                            })
                            .ToList();
                    }
                    else
                    {
                        MessageBox.Show("PersonalInformation file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

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
                // Save user data to the file (CSV format)
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "users.csv");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllLines(filePath, users.Select(u => $"{u.Username},{u.Password},{u.UserType}"));


                // Save user PersonalInformation to the file (CSV format)
                filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "PersonalInformation.csv");
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllLines(filePath, users.Select(u => $"{u.PersonalInfo.Username},{u.PersonalInfo.Name},{u.PersonalInfo.Surname},{u.PersonalInfo.Email},{u.PersonalInfo.Phone},{u.PersonalInfo.Address},{u.PersonalInfo.ProfilePicture}"));
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

            if (newPassword != txtNewPasswordVerify.Text)
            {
                MessageBox.Show("Passwords do not match. Please verify the password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Check if the personal information is valid using regular expressions
            if (!Regex.IsMatch(tbName.Text, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("Name is not valid. Please enter a valid name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Surname
            if (!Regex.IsMatch(tbSurname.Text, @"^[a-zA-Z]+$"))
            {
                MessageBox.Show("Surname is not valid. Please enter a valid surname.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Email
            if (!Regex.IsMatch(tbEmail.Text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                MessageBox.Show("Email is not valid. Please enter a valid email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                //Check if the email is already registered
                if (users.Any(u => u.PersonalInfo.Email == tbEmail.Text))
                {
                    MessageBox.Show("Email is already registered. Please enter a different email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            //Phone
            if (!Regex.IsMatch(tbPhone.Text, @"^(\d{10})$"))
            {
                MessageBox.Show("Phone number is not valid. Please enter a valid phone number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Address
            if (!Regex.IsMatch(tbAddress.Text, @"^[a-zA-Z0-9\s,.'-]{3,}$"))
            {
                MessageBox.Show("Address is not valid. Please enter a valid address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // Create a new user and add to the list
            var newPersonalInfo = new PersonalInformation
            {
                Username = newUsername,
                Name = tbName.Text,
                Surname = tbSurname.Text,
                Email = tbEmail.Text,
                Phone = tbPhone.Text,
                Address = tbAddress.Text,
                ProfilePicture = "R0lGODlhAQABAIAAAAAAAAAAACH5BAAAAAAALAAAAAABAAEAAAICTAEAOw==" // Default profile picture
            };
            var newUser = new User
            {
                Username = newUsername,
                Password = newPassword,
                UserType = users.Count == 0 ? UserType.Admin : UserType.NormalUser, // First user is admin
                PersonalInfo = newPersonalInfo
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


        //Register operations
        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            //Show register groupbox and hide login groupbox
            groupBoxRegister.Visible = true;
            groupBoxLogin.Visible = false;

        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            //Show login groupbox and hide register groupbox
            groupBoxRegister.Visible = false;
            groupBoxLogin.Visible = true;
        }


    }
}

