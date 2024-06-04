using System;
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
        private List<string> notes; // A list for taking notes  akyldrmbyznr
        
        private List<Reminder> reminders; // for reminder  akyldrmbyznr 
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

            //Notes 
            notes = new List<string>();   //akyldrmbyznr
            LoadNotesFromFile();          //akyldrmbyznr
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
        // akyldrmbyznr codes of notes section
        private void LoadNotesFromFile()     // This method reads the notes from the file and loads them into the notes list in memory.
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "notes.csv");
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        notes.Add(line);
                    }
                }
            }
        }
        private void SaveNotesToFile() // this metod saves the notes.
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "notes.csv");
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var note in notes)
                {
                    writer.WriteLine(note);
                }
            }
        }
        private void RefreshNotes() //this method is a method used to update the notes list (ListBox) and save the changes to the file that use SaveNotesToFile metod.
        {
            listBoxNotes.Items.Clear();
            foreach (var note in notes)
            {
                listBoxNotes.Items.Add(note);
            }

            SaveNotesToFile();
        }

        private void btnAddNote_Click(object sender, EventArgs e) // this method adds a new note to the list.
        {
            if (string.IsNullOrWhiteSpace(textBoxNoteContent.Text))
            {
                MessageBox.Show("Lütfen bir not girin.");
                return;
            }

            notes.Add(textBoxNoteContent.Text);
            textBoxNoteContent.Clear();
            RefreshNotes();
        }

        private void btnUpdateNote_Click(object sender, EventArgs e) // this method updates the selected note.
        {
            if (listBoxNotes.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen güncellemek için bir not seçin.");
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxNoteContent.Text))
            {
                MessageBox.Show("Not içeriği boş olamaz.");
                return;
            }

            notes[listBoxNotes.SelectedIndex] = textBoxNoteContent.Text;
            textBoxNoteContent.Clear();
            RefreshNotes();
        }

        private void btnDeleteNote_Click(object sender, EventArgs e) //this method delete the selected notes.
        {
            if (listBoxNotes.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen silmek için bir not seçin.");
                return;
            }

            notes.RemoveAt(listBoxNotes.SelectedIndex);
            textBoxNoteContent.Clear();
            RefreshNotes();
        }


        private void listBoxNotes_SelectedIndexChanged(object sender, EventArgs e) //When an item is selected in listBoxNotes, it checks the selected item.
        {                                                                          //If an item is selected, it retrieves the selected item from the notes list 
            if (listBoxNotes.SelectedIndex != -1)                                  //and writes it into the textBoxNoteContent text box.
            {
                textBoxNoteContent.Text = notes[listBoxNotes.SelectedIndex];
            }
        }

        private void btnListNote_Click(object sender, EventArgs e) // this method lists all notes. 
        {
            RefreshNotes();
        }
        

        // Salary Calculator 

        // salary hesaplanması için ihtiyacımız olan katsayıların tanımlanması
        private double deneyimK = 0.0;
        private double ilK = 0.0;
        private double ustK = 0.0;
        private double YonetK = 0.0;
        private double DilK = 0.0;
        private double AileK = 0.0;
        private double calismaTuru = 1;

        // baz ücret için belgeden alınan değer
        private double bazUcret = 26828;
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            
            if (radioButton1 != null) // deneyim varsa combobox açılır ve deneyim süresi seçilir
            {
                comboBoxDeneyim.Visible = true;
            }
        }

        private void radioButtonUstEvet_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButtonUstEvet != null) // üst öğrenim varsa combobox açılır ve üst öğrenim türü seçilir
            {
                comboBoxUstOgr.Visible = true;
            }
        }
        
        private void radioButtonYonetEvet_CheckedChanged(object sender, EventArgs e)
        {
            if((radioButtonYonetEvet != null)) // Yöneticilik görevi varsa combobox açılır ve tür seçilir
            {
                comboBoxYonetici.Visible = true;
            }
        }

        private void radioButtonDilVar_CheckedChanged(object sender, EventArgs e)
        {
            if ((radioButtonDilVar != null)) // Yabancı dil için combobox açılır
            { 
                comboBoxDil.Visible = true;
            }
        }

        private void radioButtonEvli_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonEvli != null) // eğer evliyse combobox açılır
            {
                comboBoxAile.Visible = true;
                    }
        }

        private void comboBoxDeneyim_SelectedIndexChanged(object sender, EventArgs e)
        {
                // Deneyim comboboxında seçilen seçeneğe göre deneyim katsayısının değeri değiştirilir
                switch (comboBoxDeneyim.SelectedIndex)
                {   
                    // sıfıncı indexte seçiniz özelliği var 
                    case 1: 
                        deneyimK = 0.60;
                        break;
                    case 2:
                        deneyimK = 1;
                        break;
                    case 3:
                        deneyimK = 1.20;
                        break;
                    case 4:
                        deneyimK = 1.35;
                        break;
                    case 5:
                        deneyimK = 1.50;
                        break;
                    default:
                        break;
                
            }
        }

        private void comboBoxIl_SelectedIndexChanged(object sender, EventArgs e)
        {
                // yaşanılan ilin seçilmesine göre il katsayısı değiştirilir
                // belgedeki iller için katsayı değerleri alfabetik sırasına göre case'lere yerleştirildi 
                switch (comboBoxIl.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        ilK = 0.05;
                        break;
                    case 7:
                        ilK = 0.20;
                        break;
                    case 8:
                        ilK = 0.05;
                        break;
                    case 10:
                        ilK = 0.05;
                        break;
                    case 11:
                        ilK = 0.05;
                        break;
                    case 12:
                        ilK = 0.05;
                        break;
                    case 16:
                        ilK = 0.05;
                        break;
                    case 19:
                        ilK = 0.10;
                        break;
                    case 20:
                        ilK = 0.05;
                        break;
                    case 21:
                        ilK = 0.05;
                        break;
                    case 22:
                        ilK = 0.05;
                        break;
                    case 25:
                        ilK = 0.05;
                        break;
                    case 27:
                        ilK = 0.10;
                        break;
                    case 28:
                        ilK = 0.10;
                        break;
                    case 32:
                        ilK = 0.05;
                        break;
                    case 34:
                        ilK = 0.05;
                        break;
                    case 35:
                        ilK = 0.05;
                        break;
                    case 39:
                        ilK = 0.05;
                        break;
                    case 40:
                        ilK = 0.30;
                        break;
                    case 41:
                        ilK = 0.20;
                        break;
                    case 50:
                        ilK = 0.10;
                        break;
                    case 52:
                        ilK = 0.10;
                        break;
                    case 58:
                        ilK = 0.05;
                        break;
                    case 59:
                        ilK = 0.05;
                        break;
                    case 63:
                        ilK = 0.05;
                        break;
                    case 65:
                        ilK = 0.05;
                        break;
                    case 66:
                        ilK = 0.10;
                        break;
                    case 73:
                        ilK = 0.10;
                        break;
                    case 75:
                        ilK = 0.05;
                        break;
                    case 79:
                        ilK = 0.10;
                        break;
                // diğerleri için katsayı 0
                    default:
                        ilK = 0;
                        break;


                
            }
        }

        private void comboBoxUstOgr_SelectedIndexChanged(object sender, EventArgs e)
        {
                // Üst öğrenim için seçilen seçeneğe göre katsayı değiştirilir
                switch (comboBoxUstOgr.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        ustK = 0.10;
                        break;
                    case 2:
                        ustK = 0.30;
                        break;
                    case 3:
                        ustK = 0.35;
                        break;
                    case 4:
                        ustK = 0.05;
                        break;
                    case 5:
                        ustK = 0.15;
                        break;
                    default:
                        break;

                
            }

        }

        private void comboBoxDil_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Seçilen dil seçeneğine göre katsayı değiştirilir
                switch (comboBoxDil.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        DilK = 0.20;
                        break;
                    case 2:
                        DilK = 0.20;
                        break;
                    case 3:
                        DilK = 0.05;
                        break;
                    default:
                        break;
                
            }
        }

        private void comboBoxYonetici_SelectedIndexChanged(object sender, EventArgs e)
        {
                // yöneticilik görevi için seçilen seçeneğe göre katsayı değiştirilir
                switch (comboBoxYonetici.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        YonetK = 0.50;
                        break;
                    case 2:
                        YonetK = 0.50;
                        break;
                    case 3:
                        YonetK = 0.50;
                        break;
                    case 4:
                        YonetK = 0.50;
                        break;
                    case 5:
                        YonetK = 0.75;
                        break;
                    case 6:
                        YonetK = 0.85;
                        break;
                    case 7:
                        YonetK = 0.85;
                        break;
                    case 8:
                        YonetK = 1;
                        break;
                    case 9:
                        YonetK = 1;
                        break;
                    case 10:
                        YonetK = 0.40;
                        break;
                    case 11:
                        YonetK = 0.60;
                        break;
                    default:
                        break;
                
            }
        }

        private void comboBoxAile_SelectedIndexChanged(object sender, EventArgs e)
        {
                // Aile seçeneği için seçilene göre katsayı değiştirilir
                switch (comboBoxAile.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        AileK = 0.20;
                        break;
                    case 2:
                        AileK = 0.20;
                        break;
                    case 3:
                        AileK = 0.30;
                        break;
                    case 4:
                        AileK = 0.40;
                        break;
                    default:
                        break;
                
            }
        }
        // akyldrmbyznr The codes for the notes section are finished.


        // salary hesaplayan fonksiyon
        // katsayıları comboboxta seçilen değere göre değiştildikten sonra burada kullanılır
        // comboboxta seçim yapılmamışsa katsayı default 0 olur
        public void salary()
        {
            // katsayılar toplamı+1
            double katsayi = AileK + YonetK + DilK + ustK + ilK + deneyimK+1;
            // baz ücret başta kabul edilen değer
            double ucret = bazUcret;
            //salary= (katsayılar+1)*baz ücret
            ucret*= katsayi;
            // full time çalışma seçilirse 1, part time seçilirse 0.5 yani yarısı hesaplanır
            ucret *= calismaTuru;
            //labela salary yazdırılır
            labelucret.Text=ucret.ToString()+" TL";
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            bool uyarıVar = false; // En az bir uyarı gösterildiğini kontrol etmek için bir boolean kullanıyoruz

            if (comboBoxAile.SelectedIndex == 0 && comboBoxAile.Visible == true)
            {
                MessageBox.Show("Lütfen aile durumu için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBox1.SelectedIndex == 0 && comboBox1.Visible == true)
            {
                MessageBox.Show("Lütfen çalışma durumu için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBoxDeneyim.SelectedIndex == 0 && comboBoxDeneyim.Visible == true)
            {
                MessageBox.Show("Lütfen deneyim için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBoxDil.SelectedIndex == 0 && comboBoxDil.Visible == true)
            {
                MessageBox.Show("Lütfen yabancı dil için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBoxIl.SelectedIndex == 0 && comboBoxIl.Visible == true)
            {
                MessageBox.Show("Lütfen yaşanılan il için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBoxUstOgr.SelectedIndex == 0 && comboBoxUstOgr.Visible == true)
            {
                MessageBox.Show("Lütfen üst öğrenim için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (comboBoxYonetici.SelectedIndex == 0 && comboBoxYonetici.Visible == true)
            {
                MessageBox.Show("Lütfen yönetici görevi için bir seçim yapın!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uyarıVar = true; // En az bir uyarı gösterildi
                labelucret.Text = "  ";
            }

            if (!uyarıVar) // Eğer hiçbir uyarı gösterilmediyse
            {
                salary(); // Fonksiyon çağrılır
                textBox8.Text = labelucret.Text;
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // çalışam türü seçilir ve katsayı değeri değiştirilir
                switch (comboBox1.SelectedIndex)
                {
                // sıfıncı indexte seçiniz özelliği var 
                    case 1:
                        calismaTuru = 1;
                        break;
                    case 2:
                        calismaTuru = 0.5;
                        break;
                    default:
                        break;
                }
            
        }

        private void tabPage4_Enter(object sender, EventArgs e)
        {
            // uyarıları gönderebilmek için her comboboxın 0. indexine seçiniz seçeneği koyuldu ve default o seçili görünür
          comboBox1.SelectedIndex = 0;
            comboBoxAile.SelectedIndex = 0;
            comboBoxDeneyim.SelectedIndex = 0;
            comboBoxDil.SelectedIndex = 0;
            comboBoxIl.SelectedIndex = 0;
            comboBoxUstOgr.SelectedIndex = 0;
            comboBoxYonetici.SelectedIndex = 0;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        //fethiyenur salary calculator kısmı bitti

        private void listBoxReminders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxReminders.SelectedIndex != -1)                                 //and type information of the reminder in the relevant fields in the form.
            {
                var selectedReminder = reminders[listBoxReminders.SelectedIndex];
                dateTimePickerReminder.Value = selectedReminder.Date.Add(selectedReminder.Time);
                textBoxReminderSummary.Text = selectedReminder.Summary;
                textBoxReminderDescription.Text = selectedReminder.Description;
                comboBoxReminderType.SelectedItem = selectedReminder.Type;
            }
        }
        private void LoadRemindersFromFile()       //This method reads reminders from the "reminders.csv" file
        {                                          //and adds them to the reminders list.
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "reminders.csv");
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var reminder = Reminder.FromCsv(line);
                        reminders.Add(reminder);
                    }
                }
            }
        }
        private void SaveRemindersToFile()        //Saves reminders to file.
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OopLabProje", "reminders.csv");
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var reminder in reminders)
                {
                    writer.WriteLine(reminder.ToCsv());
                }
            }
        }
        private void RefreshReminders()           //This method clears the reminders from the list and adds all the reminders
        {                                         //in the reminders list to the list, then saves these reminders to the file.
            listBoxReminders.Items.Clear();
            foreach (var reminder in reminders)
            {
                listBoxReminders.Items.Add(reminder.Summary);
            }

            SaveRemindersToFile();
        }

        private void btnAddReminder_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxReminderType.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir hatırlatıcı türü seçin.");
                    return;
                }

                var reminder = new Reminder
                {
                    Date = dateTimePickerReminder.Value.Date,
                    Time = dateTimePickerReminder.Value.TimeOfDay,
                    Summary = textBoxReminderSummary.Text,
                    Description = textBoxReminderDescription.Text,
                    Type = (ReminderType)comboBoxReminderType.SelectedItem
                };

                reminders.Add(reminder);
                RefreshReminders();
                ClearReminderFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hatırlatıcı eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (listBoxReminders.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen güncellemek için bir hatırlatıcı seçin.");
                return;
            }

            var selectedReminder = reminders[listBoxReminders.SelectedIndex];
            selectedReminder.Date = dateTimePickerReminder.Value.Date;
            selectedReminder.Time = dateTimePickerReminder.Value.TimeOfDay;
            selectedReminder.Summary = textBoxReminderSummary.Text;
            selectedReminder.Description = textBoxReminderDescription.Text;
            selectedReminder.Type = (ReminderType)comboBoxReminderType.SelectedItem;

            RefreshReminders();
            ClearReminderFields();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBoxReminders.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen silmek için bir hatırlatıcı seçin.");
                return;
            }

            reminders.RemoveAt(listBoxReminders.SelectedIndex);
            RefreshReminders();
            ClearReminderFields();
        }
        private void ClearReminderFields()
        {
            textBoxReminderSummary.Clear();
            textBoxReminderDescription.Clear();
            comboBoxReminderType.SelectedIndex = -1;
        }

        private void btnShowReminder_Click(object sender, EventArgs e)
        {
            if (listBoxReminders.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen bir hatırlatıcı seçin.");
                return;
            }

            var selectedReminder = reminders[listBoxReminders.SelectedIndex];
            ShowReminder(selectedReminder);
        }
        private void ShowReminder(Reminder reminder)
        {
            // Pencere başlığında hatırlatıcı özetini göster
            this.Text = reminder.Summary;

            // Pencereyi 2 saniye boyunca titreştir
            for (int i = 0; i < 20; i++)
            {
                this.Left += i % 2 == 0 ? 10 : -10;
                System.Threading.Thread.Sleep(50);
            }
            this.Left = (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2;
        }


        // akyldrmbyznr The codes for the reminder section are finished.
    }
    public class Reminder
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public ReminderType Type { get; set; }

        public string ToCsv()
        {
            return $"{Date.ToShortDateString()},{Time},{Summary},{Description},{Type}";
        }

        public static Reminder FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return new Reminder
            {
                Date = DateTime.Parse(values[0]),
                Time = TimeSpan.Parse(values[1]),
                Summary = values[2],
                Description = values[3],
                Type = (ReminderType)Enum.Parse(typeof(ReminderType), values[4])
            };
        }
    }

    public enum ReminderType
    {
        Meeting,
        Task
    }

}
