using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace BalahninLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client _currentClients = new Client();
        private bool _isEditMode = false;
        public AddEditPage()
        {
            InitializeComponent();
            _currentClients = new Client(); // ID сгенерируется автоматически
            _isEditMode = false;
            DataContext = _currentClients;
            DateBirthday.SelectedDate = DateTime.Now;
            LblID.Visibility = Visibility.Collapsed;
            TxtID.Visibility = Visibility.Collapsed;


        }
        public AddEditPage(Client SelectedClient)
        {
            InitializeComponent();
            if (SelectedClient != null)
            {
                _currentClients = SelectedClient;
                _isEditMode = true;
            }
            DataContext = _currentClients;
            DateBirthday.SelectedDate = DateTime.Now;

            LblID.Visibility = Visibility.Visible;
            TxtID.Visibility = Visibility.Visible;
            TxtID.IsReadOnly = true;  // Нельзя редактировать
            TxtID.Background = Brushes.LightGray;
            LoadClientData();
        }
        private void LoadClientData()
        {
            // Загружаем пол
            if (_currentClients.GenderCode == "м")
                RButtonMens.IsChecked = true;
            else if (_currentClients.GenderCode == "ж")
                RButtonWoman.IsChecked = true;

            // Загружаем дату рождения
            if (_currentClients.Birthday.HasValue)
            {
                DateBirthday.SelectedDate = _currentClients.Birthday.Value;
            }


            // ЗАГРУЖАЕМ ФОТО
            LoadPhoto();
        }

        private void LoadPhoto()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentClients.PhotoPath))
                {
                    LogoImage.Source = null;  // Просто null, ничего не загружаем
                    return;
                }

                string fileName = Path.GetFileName(_currentClients.PhotoPath);
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", fileName);

                // Ищем в других местах
                if (!File.Exists(fullPath))
                {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                }

                if (!File.Exists(fullPath))
                {
                    string projectPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
                    fullPath = Path.Combine(projectPath, "Клиенты", fileName);
                }

                if (File.Exists(fullPath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    LogoImage.Source = bitmap;
                }
                else
                {
                    LogoImage.Source = null;  // Не нашли - null
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки фото: {ex.Message}");
                LogoImage.Source = null;
            }
        }
        private string GetSelectedGender()
        {
            if (RButtonMens.IsChecked == true)
                return "м";
            if (RButtonWoman.IsChecked == true)
                return "ж";
            return null;
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Проверка на русские буквы
            if (System.Text.RegularExpressions.Regex.IsMatch(email, @"[а-яА-Я]"))
                return false;

            // Простая проверка: должен быть @ и точка после @
            if (!email.Contains("@") || !email.Contains("."))
                return false;

            // Проверка, что @ не первый и не последний символ
            int atIndex = email.IndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
                return false;

            // Проверка, что после @ есть точка
            string domain = email.Substring(atIndex + 1);
            if (!domain.Contains("."))
                return false;

            // Проверка, что точка не первый символ в домене
            int dotIndex = domain.IndexOf('.');
            if (dotIndex <= 0 || dotIndex == domain.Length - 1)
                return false;

            return true;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (DateBirthday.SelectedDate == null)
            {
                _currentClients.Birthday = DateTime.Now;
            }
            else
            {
                _currentClients.Birthday = DateBirthday.SelectedDate;
            }
            _currentClients.GenderCode = GetSelectedGender();
            if (!_isEditMode)
            {
                _currentClients.RegistrationDate = DateTime.Now;
            }


            if (string.IsNullOrWhiteSpace(_currentClients.LastName))
                errors.AppendLine("Необходимо написать фамилию");
            else if (_currentClients.LastName.Length > 50)
                errors.AppendLine("Фамилия не может быть длиннее 50 символов");
            if (string.IsNullOrWhiteSpace(_currentClients.FirstName))
                errors.AppendLine("Необходимо написать имя");
            else if (_currentClients.FirstName.Length > 50)
                errors.AppendLine("Имя не может быть длиннее 50 символов");
            if (string.IsNullOrWhiteSpace(_currentClients.Patronymic))
                errors.AppendLine("Необходимо написать отчество");
            else if (_currentClients.Patronymic.Length > 50)
                errors.AppendLine("Отчество не может быть длиннее 50 символов");
            // Проверка Email
            if (string.IsNullOrWhiteSpace(_currentClients.Email))
            {
                errors.AppendLine("Email не может быть пустым");
            }
            else if (!IsValidEmail(_currentClients.Email))
            {
                errors.AppendLine("Введите корректный email (пример: user@mail.ru)");
            }
            // Проверка телефона
            if (string.IsNullOrWhiteSpace(_currentClients.Phone))
            {
                errors.AppendLine("Укажите телефон");
            }
            else
            {
                // Удаляем разрешенные символы: +, -, (, ), пробел
                string cleanPhone = _currentClients.Phone
                    .Replace("+", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(" ", "");

                // Проверяем, что остались только цифры
                if (!cleanPhone.All(char.IsDigit))
                {
                    errors.AppendLine("Телефон может содержать только цифры и символы: +, -, (, ), пробел");
                }
                else if (cleanPhone.Length < 10 || cleanPhone.Length > 12)
                {
                    errors.AppendLine("Телефон должен содержать 10-12 цифр");
                }
            }


            string gender = GetSelectedGender();

            if (string.IsNullOrWhiteSpace(gender))
            {
                errors.AppendLine("Необходимо выбрать пол");
            }
            else
            {
                _currentClients.GenderCode = gender;
            }
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка валидации",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Проверка на NULL для обязательных полей
            if (_currentClients.RegistrationDate == null)
            {
                MessageBox.Show("RegistrationDate = NULL! Устанавливаем текущую дату.");
                _currentClients.RegistrationDate = DateTime.Now;
            }
            // СОХРАНЕНИЕ В БАЗУ
            try
            {
                var context = BalahninLanguageEntities.GetContext();

                if (_isEditMode)
                {
                    var clientFromDb = context.Client.Find(_currentClients.ID);
                    if (clientFromDb != null)
                    {
                        clientFromDb.LastName = _currentClients.LastName;
                        clientFromDb.FirstName = _currentClients.FirstName;
                        clientFromDb.Patronymic = _currentClients.Patronymic;
                        clientFromDb.Email = _currentClients.Email;
                        clientFromDb.Phone = _currentClients.Phone;
                        clientFromDb.Birthday = _currentClients.Birthday;
                        clientFromDb.GenderCode = _currentClients.GenderCode;
                        clientFromDb.PhotoPath = _currentClients.PhotoPath;
                    }
                }
                else
                {
                    context.Client.Add(_currentClients);
                }

                context.SaveChanges();

                MessageBox.Show("Сохранено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                string msg = "Ошибка: " + ex.Message;

                // Выводим также тип ошибки
                msg += $"\n\nТип ошибки: {ex.GetType().Name}";

                MessageBox.Show(msg, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();
            myOpenFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*";

            if (myOpenFileDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFile = myOpenFileDialog.FileName;
                    if (!File.Exists(sourceFile))
                    {
                        MessageBox.Show("Исходный файл не найден.");
                        return;
                    }

                    string clientsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты");
                    Directory.CreateDirectory(clientsFolder);

                    string fileName = Path.GetFileName(sourceFile);
                    string destPath = Path.Combine(clientsFolder, fileName);

                    File.Copy(sourceFile, destPath, true);

                    // Сохраняем путь в БД
                    _currentClients.PhotoPath = $@"Клиенты\{fileName}";

                    // ПОКАЗЫВАЕМ ФОТО СРАЗУ
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(destPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    LogoImage.Source = bitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при копировании файла: " + ex.Message);
                }
            }
        }

        private void RadioButton_RButtonMens(object sender, RoutedEventArgs e)
        {
        }

        private void RadioButton_RButtonWoman(object sender, RoutedEventArgs e)
        {
        }
    }
}
