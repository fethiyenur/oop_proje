﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OopLabProje.LoginForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace OopLabProje
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            //Hide tabpage2 if user is not admin
            if (LoginForm.currentUser.UserType != LoginForm.UserType.Admin)
            {
                tabControl1.TabPages.Remove(tabPage2);
            }
            else
            {
                FillUserComboBox();
            }

            //Phonebook 
            contacts = new List<Contact>();

            CheckSaveDirectory();
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "Contacts.txt");
            LoadContactsFromFile(path);
        }

        //Seperate into classes
        private class UserManagement
        {

        }

        private void FillUserComboBox()
        {
            // Clear the combobox
            cbSelectUser.Items.Clear();

            //Fill the combobox with all users except the current user
            foreach (var user in LoginForm.users)
            {
                if (user != LoginForm.currentUser)
                {
                    cbSelectUser.Items.Add(user.Username);
                }
            }

            // Select the first user
            if (cbSelectUser.Items.Count > 0)
            {
                cbSelectUser.SelectedIndex = 0;
            }
        }

        private void btnChangeUserType_Click(object sender, EventArgs e)
        {
            // Get selected user from the grid
            var selectedUser = GetSelectedUser();

            if (selectedUser != null)
            {
                //Change the selected user type with the type from cbUserType combobox
                selectedUser.UserType = (UserType)cbUserType.SelectedIndex;

                // Update the user type in memory and save to file
                LoginForm.Instance.SaveUsersToFile();

                //Display user Saved message if succesfull
                MessageBox.Show("User Saved");

            }
        }

        private User GetSelectedUser()
        {

            //return selected user from cbSelectUser combobox (find the user from users)
            return LoginForm.users.Find(user => user.Username == cbSelectUser.SelectedItem.ToString());

        }

        private void cbSelectUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            //select the cbUserType combobox with the selected user's type
            var selectedUser = GetSelectedUser();
            if (selectedUser != null)
            {
                cbUserType.SelectedIndex = (int)selectedUser.UserType;
            }

        }


        //Phonebook Start
        public class Contact
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Description { get; set; }
            public string Email { get; set; }
        }
        private List<Contact> contacts;


        public void LoadContactsFromFile(string filePath)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] columns = line.Split(',');

                        var contact = new Contact
                        {
                            Name = columns[0],
                            Surname = columns[1],
                            PhoneNumber = columns[2],
                            Address = columns[3],
                            Description = columns[4],
                            Email = columns[5]
                        };

                        contacts.Add(contact);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading contacts: {ex.Message}");
            }
        }

        public void SaveContactsToFile(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var contact in contacts)
                    {
                        writer.WriteLine($"{contact.Name},{contact.Surname},{contact.PhoneNumber},{contact.Address},{contact.Description},{contact.Email}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving contacts: {ex.Message}");
            }
        }

        void CheckSaveDirectory()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //if Contacts.txt does not exist, create it
            string filePath = Path.Combine(path, "Contacts.txt");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }

        private void btnListContacts_Click(object sender, EventArgs e)
        {
            // Clear existing items in the ListBox
            listBoxContacts.Items.Clear();

            // Populate the ListBox with contact names
            foreach (var contact in contacts)
            {
                listBoxContacts.Items.Add($"{contact.Name} | {contact.Surname} | {contact.PhoneNumber} | {contact.Address} | {contact.Description} | {contact.Email}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            Contact contact = new Contact
            {
                Name = textBox5.Text,
                Surname = textBox4.Text,
                PhoneNumber = textBox3.Text,
                Address = textBox2.Text,
                Description = textBox1.Text,
                Email = textBox6.Text
            };
            contacts.Add(contact);


            RefreshContacts();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBoxContacts.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a contact to delete.");
                return;
            }


            Contact tempContact = contacts[listBoxContacts.SelectedIndex];
            DeleteContact(tempContact);
        }

        void DeleteContact(Contact contact) 
        {

            // Specify the email address to delete
            string emailToDelete = contact.Email;

            // Find the contact with the specified email address
            Contact contactToDelete = contacts.Find(c => c.Email == emailToDelete);

            if (contactToDelete != null)
            {
                // Remove the contact from the list
                contacts.Remove(contactToDelete);

                // Write the updated contacts back to the file
                RefreshContacts();
                Console.WriteLine($"Contact with email '{emailToDelete}' deleted successfully.");
            }
            else
            {
                Console.WriteLine($"Contact with email '{emailToDelete}' not found.");
            }
        }

        void RefreshContacts()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "Contacts.txt");
            File.WriteAllText(path, string.Empty);
            SaveContactsToFile(path);
            contacts.Clear();
            LoadContactsFromFile(path);
            btnListContacts_Click(null, null);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBoxContacts.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a contact to update.");
                return;
            }

            if (!ValidateInputs())
            {
                return;
            }

            UpdateContact(listBoxContacts.SelectedIndex);
        }

        void UpdateContact(int index)
        {
            Contact contact = new Contact
            {
                Name = textBox5.Text,
                Surname = textBox4.Text,
                PhoneNumber = textBox3.Text,
                Address = textBox2.Text,
                Description = textBox1.Text,
                Email = textBox6.Text
            };
            contacts[index] = contact;

            RefreshContacts();
        }

        private void listBoxContacts_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox5.Text = contacts[listBoxContacts.SelectedIndex].Name;
            textBox4.Text = contacts[listBoxContacts.SelectedIndex].Surname;
            textBox3.Text = contacts[listBoxContacts.SelectedIndex].PhoneNumber;
            textBox2.Text = contacts[listBoxContacts.SelectedIndex].Address;
            textBox1.Text = contacts[listBoxContacts.SelectedIndex].Description;
            textBox6.Text = contacts[listBoxContacts.SelectedIndex].Email;
        }

        bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(textBox5.Text) || string.IsNullOrEmpty(textBox4.Text) || string.IsNullOrEmpty(textBox3.Text) || string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox6.Text))
            {
                MessageBox.Show("Please fill all the fields.");
                return false;
            }

            //validate each text field using regular expressions
            if (!System.Text.RegularExpressions.Regex.IsMatch(textBox5.Text, "^[a-zA-Z]+$"))
            {
                MessageBox.Show("Name can only contain letters.");
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(textBox4.Text, "^[a-zA-Z]+$"))
            {
                MessageBox.Show("Surname can only contain letters.");
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(textBox3.Text, @"^(\+[0-9]{1,3})?[0-9]{10}$"))
            {
                MessageBox.Show("Phone number must be 10 digits.");
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(textBox6.Text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                MessageBox.Show("Email is not valid.");
                return false;
            }


            return true;
        }




        //Personal Information Start
        private void tabPage3_Click(object sender, EventArgs e)
        {
            LoadUserInfo();
        }

        //Load the current user's information into the textboxes
        //the tex file we use to store is in the format of Username,Password,UserType,Name,Surname,PhoneNumber,Address,Email
        //load the current user's information from the file by finding the same username
        private void LoadUserInfo()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "Users.txt");
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] columns = line.Split(',');

                    if (columns[0] == LoginForm.currentUser.Username)
                    {
                        textBox12.Text = columns[3];
                        textBox11.Text = columns[4];
                        textBox10.Text = columns[5];
                        textBox9.Text = columns[6];
                        textBox7.Text = columns[7];
                    }
                }
            }
        }



    }

}