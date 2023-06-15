using Aardvark.Base.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Aardvark.Base.Threading;
using Path = System.IO.Path;

namespace BudGet2._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Zametka> notes = new ObservableCollection<Zametka>();
        public static ObservableCollection<string> typesOfNote = new ObservableCollection<string>();
        public MainWindow()
        {
            bool fileExists = CheckZametkiFileExists();

            if (fileExists)
            {
                
            }
            else
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, "zametki.json");

                File.Create(filePath).Close();
            }
            InitializeComponent();
            Date.SelectedDate = DateTime.Today;
            UpdateDataGrid();
            LoadTypes();
            LoadData();
            note_Type.ItemsSource = typesOfNote;
        }
        public bool CheckZametkiFileExists()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "zametki.json");

            return File.Exists(filePath);
        }

        private void LoadData()
        {
            var desereolize = Json.desereolize<ObservableCollection<Zametka>>();
            if (desereolize != null && desereolize.Count != 0)
            {
                notes = Json.desereolize<ObservableCollection<Zametka>>();
            }
        }
        private void LoadTypes()
        {
            var dict = Json.desereolize<Dictionary<string, List<Zametka>>>();

            var uniqueTypes = new HashSet<string>();
            if (dict == null || dict.Count == 0)
            {
                
            }
            else
            {
                foreach (var date in dict)
                {
                    foreach (var zametka in date.Value)
                    {
                        if (!uniqueTypes.Contains(zametka.Type_zametka))
                        {
                            uniqueTypes.Add(zametka.Type_zametka);
                        }
                    }
                }

                foreach (var type in uniqueTypes)
                {
                    typesOfNote.Add(type);

                }
            }
            
        }
        private void UpdateDataGrid()
        {


            var deserializedData = Json.desereolize<Dictionary<string, ObservableCollection<Zametka>>>();
            if (deserializedData == null || deserializedData.Count == 0)
            {
                Dictionary<string, List<Zametka>> data = new Dictionary<string, List<Zametka>>();
                data["14.05.2023"] = new List<Zametka>();
                Json.sereolize(data);
            }
            else
            {
                if (deserializedData.TryGetValue(Date.SelectedDate.Value.ToString("dd.MM.yyyy"), out var notes1))
                {
                    DataGrid.ItemsSource = notes1;
                }
                else
                {
                    DataGrid.ItemsSource = null;
                }
                CalculateTotalAmountOfMoney();
            }
            
        }

        private void SaveData()
        {
            Json.sereolize(notes);
        }

        private void Button_delete_Click(object sender, RoutedEventArgs e)
        {
            var dict = Json.desereolize<Dictionary<string, List<Zametka>>>();

            if (dict.ContainsKey(Date.SelectedDate.Value.ToString("dd.MM.yyyy")))
            {
                var note = dict[Date.SelectedDate.Value.ToString("dd.MM.yyyy")].FirstOrDefault(x => x.name_zametka == note_name.Text);

                if (note != null)
                {
                    dict[Date.SelectedDate.Value.ToString("dd.MM.yyyy")].Remove(note);

                    Json.sereolize(dict);

                    DataGrid.ItemsSource = dict[Date.SelectedDate.Value.ToString("dd.MM.yyyy")];
                }
            }

            note_Type.SelectedIndex = -1;
            note_name.Text = null;
            Sum.Text = null;
            UpdateDataGrid();
        }








        private void Data_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataGrid.SelectedItems.Count > 0)
            {
                var selectedNote = DataGrid.SelectedItem as Zametka;

                note_name.Text = selectedNote.name_zametka;
                note_Type.Text = selectedNote.Type_zametka;
                Sum.Text = selectedNote.amountOfMoney.ToString();
            }
        }

        private void Button_add_type_Click(object sender, RoutedEventArgs e)
        {
            CreateType createType = new CreateType();
            createType.Show();
            typesOfNote.Clear();
            note_Type.ItemsSource = typesOfNote;
            LoadTypes();
        }

        private void Button_change_Click(object sender, RoutedEventArgs e)
        {
            var selectedZametka = (Zametka)DataGrid.SelectedItem;

            if (selectedZametka == null)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }

            var selectedDate = Date.SelectedDate.Value.ToString("dd.MM.yyyy");

            var desereolize = Json.desereolize<Dictionary<string, List<Zametka>>>();
            if (!desereolize.ContainsKey(selectedDate))
            {
                MessageBox.Show($"Нет записей на дату {selectedDate}.");
                return;
            }
            var zametki = desereolize[selectedDate];

            var existingZametka = zametki.FirstOrDefault(z => z.name_zametka == selectedZametka.name_zametka);
            if (existingZametka == null)
            {
                MessageBox.Show($"Запись с именем {selectedZametka.name_zametka} не найдена на дату {selectedDate}.");
                return;
            }
            existingZametka.name_zametka = note_name.Text;
            existingZametka.Type_zametka = note_Type.Text;
            existingZametka.amountOfMoney = double.Parse(Sum.Text);
            existingZametka.Vichrt = Convert.ToInt32(Sum.Text) < 0 ? false : true;
            existingZametka.TIme_disk = selectedDate;

            Json.sereolize(desereolize);
            UpdateDataGrid();
        }

        private void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataGrid();
        }

        public double CalculateTotalAmountOfMoney()
        {
            var dict = Json.desereolize<Dictionary<string, ObservableCollection<Zametka>>>();
            double totalAmount = 0.0;

            foreach (var date in dict)
            {
                foreach (var note in date.Value)
                {
                    if (note.Vichrt)
                    {
                        totalAmount += note.amountOfMoney;
                    }
                    else
                    {
                        totalAmount -= note.amountOfMoney;
                    }
                }
            }
            TotalCostTextBlock.Text = totalAmount.ToString();
            return default;
        }



        private void Button_add_Click(object sender, RoutedEventArgs e)
        {
            var deserializedJson = Json.desereolize<Dictionary<string, List<Zametka>>>();
            if (deserializedJson == null || deserializedJson.Count == 0)
            {
                Zametka newNote = new Zametka();
                newNote.name_zametka = note_name.Text;
                newNote.Type_zametka = note_Type.Text;
                newNote.amountOfMoney = double.Parse(Sum.Text);
                newNote.Vichrt = newNote.amountOfMoney < 0 ? false : true; // Тернарный оператор для присвоения значения Vichrt
                newNote.TIme_disk = Date.SelectedDate.Value.ToString("dd.MM.yyyy");

                List<Zametka> newDate = new List<Zametka>();
                newDate.Add(newNote);
                deserializedJson.Add(newNote.TIme_disk, newDate);

                Json.sereolize(deserializedJson);
            }
            else
            {
                if (deserializedJson.ContainsKey(Date.SelectedDate.Value.ToString("dd.MM.yyyy")))
                {
                    Zametka newNote = new Zametka();
                    newNote.name_zametka = note_name.Text;
                    newNote.Type_zametka = note_Type.Text;
                    newNote.amountOfMoney = double.Parse(Sum.Text);
                    newNote.Vichrt = newNote.amountOfMoney < 0 ? false : true; // Тернарный оператор для присвоения значения Vichrt
                    newNote.TIme_disk = Date.SelectedDate.Value.ToString("dd.MM.yyyy");

                    deserializedJson[Date.SelectedDate.Value.ToString("dd.MM.yyyy")].Add(newNote);

                    Json.sereolize(deserializedJson);

                }
                else
                {
                    Zametka newNote = new Zametka();
                    newNote.name_zametka = note_name.Text;
                    newNote.Type_zametka = note_Type.Text;
                    newNote.amountOfMoney = double.Parse(Sum.Text);
                    newNote.Vichrt = newNote.amountOfMoney < 0 ? false : true; // Тернарный оператор для присвоения значения Vichrt
                    newNote.TIme_disk = Date.SelectedDate.Value.ToString("dd.MM.yyyy");

                    List<Zametka> newDate = new List<Zametka>();
                    newDate.Add(newNote);
                    deserializedJson.Add(newNote.TIme_disk, newDate);

                    Json.sereolize(deserializedJson);
                }
            }
            
            UpdateDataGrid();
            note_Type.SelectedIndex = -1;
            note_name.Text = null;
            Sum.Text = null;
            var des1 = Json.desereolize<Dictionary<string, List<Zametka>>>();
            if (des1 != null)
            {
                if (des1.Keys.Contains(Date.SelectedDate.Value.ToString("dd.MM.yyyy")))
                {
                    List<Zametka> next = des1[Date.SelectedDate.Value.ToString("dd.MM.yyyy")];
                    List<string> names1 = new List<string>();
                    foreach (Zametka n in next)
                    {
                        names1.Add(n.name_zametka);
                    }

                    DataGrid.ItemsSource = names1;
                }







            }
            UpdateDataGrid();
        }
    }
    internal class Zametka
    {
        /// <summary>
        /// имени, типа записи, количества денег, поступление или вычет (буленовская переменная) и даты заметки
        /// </summary>
        public string name_zametka;
        public string Type_zametka;
        public double amountOfMoney;
        public bool Vichrt;
        public string TIme_disk;
        private string _name_zametka;

        public string Name
        {
            get { return name_zametka; }
            set
            {
                if (name_zametka != value)
                {
                    name_zametka = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }



        public string Type
        {
            get { return Type_zametka; }
            set
            {
                if (Type_zametka != value)
                {
                    Type_zametka = value;
                    NotifyPropertyChanged("Type_zametka");
                }
            }
        }

        public double Money
        {
            get { return amountOfMoney; }
            set
            {
                if (amountOfMoney != value)
                {
                    amountOfMoney = value;
                    NotifyPropertyChanged("amountOfMoney");
                }
            }
        }

        public bool Plus
        {
            get { return Vichrt; }
            set
            {
                if (Vichrt != value)
                {
                    Vichrt = value;
                    NotifyPropertyChanged("Vichrt");
                }
            }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
    internal class Json
    {
        public static string file = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\zametki.json";

        public static void sereolize<T>(T znachenie)
        {
            try
            {
                string json = JsonConvert.SerializeObject(znachenie);
                File.WriteAllText(file, json);
            }
            catch (Exception)
            {

            }
        }

        public static T desereolize<T>()
        {
            try
            {
                Debug.WriteLine(file);
                string json = File.ReadAllText(file);
                T notes = JsonConvert.DeserializeObject<T>(json);
                return notes;
            }
            catch (Exception)
            {
            }
            return default(T);
        }
    }
}
