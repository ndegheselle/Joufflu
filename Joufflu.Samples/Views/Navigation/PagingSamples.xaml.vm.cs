using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Navigation
{
    public class PagingSampleItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string City { get; set; } = "";
    }

    public class PagingSamplesViewModel : ObservableObject
    {
        private static readonly string[] _firstNames =
            ["Ada", "Alan", "Grace", "Katherine", "Edsger", "Barbara", "Donald", "Linus"];
        private static readonly string[] _lastNames =
            ["Lovelace", "Turing", "Hopper", "Johnson", "Dijkstra", "Liskov", "Knuth", "Torvalds"];
        private static readonly string[] _cities =
            ["Paris", "London", "Berlin", "Madrid", "Rome", "Lisbon", "Vienna", "Oslo"];

        /// <summary>
        /// Stands in for the full data set (a database table, an API result, ...).
        /// Only the current page of it is ever handed to the DataGrid.
        /// </summary>
        private readonly List<PagingSampleItem> _source = Enumerable
            .Range(1, 123)
            .Select(
                i => new PagingSampleItem
                {
                    Id = i,
                    Name = $"{_firstNames[i % _firstNames.Length]} {_lastNames[i % _lastNames.Length]}",
                    City = _cities[i % _cities.Length]
                })
            .ToList();

        private int _pageNumber = 1;
        private int _capacity = 10;

        /// <summary>Bound two way on <c>Paging.PageNumber</c>.</summary>
        public int PageNumber
        {
            get => _pageNumber;
            set
            {
                if (SetProperty(ref _pageNumber, value))
                    UpdatePage();
            }
        }

        /// <summary>Bound two way on <c>Paging.Capacity</c> (the page size combo box).</summary>
        public int Capacity
        {
            get => _capacity;
            set
            {
                if (SetProperty(ref _capacity, value))
                    UpdatePage();
            }
        }

        public int Total => _source.Count;

        /// <summary>The rows displayed by the DataGrid, refreshed on every paging change.</summary>
        public ObservableCollection<PagingSampleItem> PageItems { get; } = [];

        public PagingSamplesViewModel() { UpdatePage(); }

        private void UpdatePage()
        {
            PageItems.Clear();
            foreach (var item in _source.Skip((PageNumber - 1) * Capacity).Take(Capacity))
                PageItems.Add(item);
        }

        public string Code => "<nav:Paging Total=\"123\" />";

        public string NoTotalCode =>
            "<!-- Total left at its -1 default -->\n" +
            "<nav:Paging\n" +
            "    PageNumber=\"{Binding PageNumber, Mode=TwoWay}\"\n" +
            "    Capacity=\"{Binding Capacity, Mode=TwoWay}\" />";

        public string DataGridCode =>
            "<DataGrid ItemsSource=\"{Binding PageItems}\" AutoGenerateColumns=\"False\">\n" +
            "    <DataGrid.Columns>\n" +
            "        <DataGridTextColumn Header=\"Id\" Binding=\"{Binding Id}\" />\n" +
            "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
            "    </DataGrid.Columns>\n" +
            "</DataGrid>\n" +
            "<nav:Paging\n" +
            "    Total=\"{Binding Total}\"\n" +
            "    PageNumber=\"{Binding PageNumber, Mode=TwoWay}\"\n" +
            "    Capacity=\"{Binding Capacity, Mode=TwoWay}\" />\n" +
            "\n" +
            "// The view model reacts to any paging change from its own setters.\n" +
            "public int PageNumber\n" +
            "{\n" +
            "    get => _pageNumber;\n" +
            "    set { if (SetProperty(ref _pageNumber, value)) UpdatePage(); }\n" +
            "}\n" +
            "\n" +
            "public int Capacity\n" +
            "{\n" +
            "    get => _capacity;\n" +
            "    set { if (SetProperty(ref _capacity, value)) UpdatePage(); }\n" +
            "}\n" +
            "\n" +
            "private void UpdatePage()\n" +
            "{\n" +
            "    // Query only the current page (Skip/Take, SQL OFFSET/FETCH, API page parameters, ...)\n" +
            "    PageItems.Clear();\n" +
            "    foreach (var item in _source.Skip((PageNumber - 1) * Capacity).Take(Capacity))\n" +
            "        PageItems.Add(item);\n" +
            "}\n" +
            "\n" +
            "// Alternatively, without binding, subscribe to the control event:\n" +
            "// paging.PagingChange += (pageNumber, capacity) => UpdatePage();";
    }
}
