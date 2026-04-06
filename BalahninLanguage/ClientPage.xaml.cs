using System;
using System.Collections.Generic;
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

namespace BalahninLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        List<Client> clients = new List<Client>();
        List<Client> TableList;

        int CurrentPage = 0;
        int RecordsPage = 10; // Значение по умолчанию
        int CountRecords;
        int CountPage;
        int TotalRecordsInDatabase; // Все записи в базе (без фильтрации)

        public ClientPage()
        {
            InitializeComponent();
            ComboGender.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            InitializeComboPage();
            UpdateClient();
            ChangePage(0, 0); // Отображаем первую страницу
            //помогло
            this.Loaded += (s, e) =>
            {
                // Обновляем при каждом возврате на страницу
                UpdateClient();
                ChangePage(0, 0);
            };
        }

        private void InitializeComboPage()
        {
            ComboPage.Items.Add(10);
            ComboPage.Items.Add(50);
            ComboPage.Items.Add(200);
            ComboPage.Items.Add("Все");
            ComboPage.SelectedIndex = 0; // По умолчанию 10
        }

        private void UpdateClient()
        {
            var clientsList = BalahninLanguageEntities.GetContext().Client.ToList();
            TotalRecordsInDatabase = clientsList.Count; // Например: 450
            var filteredClients = clientsList;

            // Фильтр по полу
            if (ComboGender.SelectedIndex == 1)
            {
                filteredClients = filteredClients.Where(p => p.GenderCode == "ж").ToList();
            }
            else if (ComboGender.SelectedIndex == 2)
            {
                filteredClients = filteredClients.Where(p => p.GenderCode == "м").ToList();
            }

            // Сортировка
            if (ComboSort.SelectedIndex == 1)
            {
                filteredClients = filteredClients.OrderBy(p => p.LastName).ToList();
            }
            else if (ComboSort.SelectedIndex == 2)
            {
                filteredClients = filteredClients.OrderByDescending(p => p.LastVisitDate).ToList();
            }
            else if (ComboSort.SelectedIndex == 3)
            {
                filteredClients = filteredClients.OrderByDescending(p => p.VisitCount).ToList();
            }

            filteredClients = filteredClients.Where(p => p.LastName.ToLower().Contains(TBox_Search.Text.ToLower())
            || p.FirstName.ToLower().Contains(TBox_Search.Text.ToLower())
            || p.Patronymic.ToLower().Contains(TBox_Search.Text.ToLower())
            || p.Email.ToLower().Contains(TBox_Search.Text.ToLower())
            || p.Phone.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").ToLower().Contains(TBox_Search.Text.Replace("+", "").Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").ToLower())).ToList();


            TableList = filteredClients;
            CountRecords = TableList.Count; // Количество после фильтрации (например: 230)
            CurrentPage = 0; // Возвращаемся на первую страницу при фильтрации
                             // ChangePage(0, 0);

        }




        private void ChangePage(int direction, int? selectedPage)
        {
            CountRecords = TableList.Count;
            CountPage = (CountRecords + RecordsPage - 1) / RecordsPage;

            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
            {
                CurrentPage = (int)selectedPage;
            }
            else
            {
                if (direction == 1 && CurrentPage > 0)
                {
                    CurrentPage--;
                }
                else if (direction == 2 && CurrentPage < CountPage - 1)
                {
                    CurrentPage++;
                }
                else
                {

                }
            }

            clients.Clear();

            int startIndex = CurrentPage * RecordsPage;
            int endIndex = Math.Min(startIndex + RecordsPage, CountRecords);
            int recordsOnPage = endIndex - startIndex;

            for (int i = startIndex; i < endIndex; i++)
            {
                clients.Add(TableList[i]);
            }

            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }

            if (CountPage > 0)
            {
                PageListBox.SelectedIndex = CurrentPage;
            }

            // Первое число - количество записей ПОСЛЕ ФИЛЬТРАЦИИ (CountRecords)
            // Второе число - общее количество в БАЗЕ (TotalRecordsInDatabase)
            TBCount.Text = CountRecords.ToString();                    // "230"
            TBAllRecords.Text = " из " + TotalRecordsInDatabase.ToString(); // " из 450"


            // Обновляем ListView
            ListViewClient.ItemsSource = clients;
            ListViewClient.Items.Refresh();
        }


        private void GoClick(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ClientPage());
        }

        private void ComboGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
            ChangePage(0, 0); // Показываем первую страницу
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
            ChangePage(0, 0); // Показываем первую страницу
        }
        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            // Получаем клиента из DataContext кнопки (так как кнопка в строке ListView)
            var button = sender as Button;
            var selectedClient = button?.DataContext as Client;

            // Если не получилось через DataContext, пробуем через выделенный элемент
            if (selectedClient == null)
            {
                selectedClient = ListViewClient.SelectedItem as Client;
            }

            if (selectedClient == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.",
                                "Внимание",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            // Проверяем наличие посещений через свойство VisitCount
            if (selectedClient.VisitCount > 0)
            {
                MessageBox.Show($"Невозможно удалить клиента.\n\n" +
                                $"У клиента \"{selectedClient.LastName} {selectedClient.FirstName}\" " +
                                $"зарегистрировано {selectedClient.VisitCount} посещения(й).\n\n" +
                                $"Последнее посещение: {selectedClient.LastVisit}\n\n" +
                                $"Сначала удалите все посещения этого клиента.",
                                "Ошибка удаления",
                                MessageBoxButton.OK);
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show($"Вы действительно хотите удалить клиента:\n\n" +
                                          $"{selectedClient.LastName} {selectedClient.FirstName} {selectedClient.Patronymic}?\n\n" +
                                          $"Действие необратимо.",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var context = BalahninLanguageEntities.GetContext();

                    // Удаляем клиента
                    context.Client.Remove(selectedClient);
                    context.SaveChanges();

                    // Обновляем список клиентов
                    UpdateClient();
                    ChangePage(0, 0); // Перезагружаем с новым RecordsPage
                    MessageBox.Show("Клиент успешно удален.",
                                    "Успех",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении клиента:\n{ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedIndex >= 0)
            {
                ChangePage(0, PageListBox.SelectedIndex);
            }
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void ComboPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboPage.SelectedIndex >= 0)
            {
                var selectedItem = ComboPage.SelectedItem;

                if (selectedItem.ToString() == "Все")
                {
                    RecordsPage = TableList.Count; // Все записи на одной странице
                }
                else if (int.TryParse(selectedItem.ToString(), out int newRecordsPage))
                {
                    RecordsPage = newRecordsPage;
                }


                CurrentPage = 0; // Возвращаемся на первую страницу
                ChangePage(0, 0); // Перезагружаем с новым RecordsPage

            }
        }

        private void TBox_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClient();
            ChangePage(0, 0); // Показываем первую страницу
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage((sender as Button).DataContext as Client));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }


    }
}
