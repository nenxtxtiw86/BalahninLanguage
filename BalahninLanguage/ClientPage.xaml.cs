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

        public ClientPage()
        {
            InitializeComponent();
            ComboGender.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            InitializeComboPage();
            UpdateClient();
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

            // Фильтр по полу
            if (ComboGender.SelectedIndex == 1)
            {
                clientsList = clientsList.Where(p => p.GenderCode == "ж").ToList();
            }
            else if (ComboGender.SelectedIndex == 2)
            {
                clientsList = clientsList.Where(p => p.GenderCode == "м").ToList();
            }

            // Сортировка
            if (ComboSort.SelectedIndex == 1)
            {
                clientsList = clientsList.OrderBy(p => p.LastName).ToList();
            }
            else if (ComboSort.SelectedIndex == 2)
            {
            }
            else if (ComboSort.SelectedIndex == 3)
            {

            }
            TableList = clientsList;
            CurrentPage = 0; // Возвращаемся на первую страницу при фильтрации
            LoadPage();
        }

     
        private void LoadPage()
        {
            clients.Clear();
            CountRecords = TableList.Count;
            CountPage = (CountRecords + RecordsPage - 1) / RecordsPage;

            // Если текущая страница больше максимальной, переходим на последнюю
            if (CurrentPage >= CountPage && CountPage > 0)
            {
                CurrentPage = CountPage - 1;
            }

            int startIndex = CurrentPage * RecordsPage;
            int endIndex = Math.Min(startIndex + RecordsPage, CountRecords);

            for (int i = startIndex; i < endIndex; i++)
            {
                clients.Add(TableList[i]);
            }

            // Обновление интерфейса
            UpdatePageListBox();
            ListViewClient.ItemsSource = clients;
            ListViewClient.Items.Refresh();
            UpdatePageInfo();
        }

        private void UpdatePageListBox()
        {
            PageListBox.Items.Clear();
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }

            if (CountPage > 0)
            {
                PageListBox.SelectedIndex = CurrentPage;
            }
        }

        private void UpdatePageInfo()
        {
            CurrentPageLabel.Content = $"Страница: {CurrentPage + 1} из {CountPage}";
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            if (selectedPage.HasValue && selectedPage >= 0 && selectedPage < CountPage)
            {
                CurrentPage = (int)selectedPage;
            }
            else // стрелки
            {
                if (direction == 1 && CurrentPage > 0) // Предыдущая страница
                {
                    CurrentPage--;
                }
                else if (direction == 2 && CurrentPage < CountPage - 1) // Следующая страница
                {
                    CurrentPage++;
                }
                else
                {
                    return; // Если не было изменений, выходим
                }
            }

            LoadPage();
        }

        private void GoClick(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ClientPage());
        }

        private void ComboGender_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
        }
        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            // Логика удаления клиента
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
                LoadPage(); // Перезагружаем данные
            }
        }
    }
}
