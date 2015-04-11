using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Music.ViewModel
{
    public class ListItem_Me
    {
        public int id { get; set; }
        public int type { get; set; }
        public string Name { get; set; }
    }
    public class Me_ListViewModel
    {
        public Me_ListViewModel()
        {
            ListItems = new ObservableCollection<ListItem_Me>();
            ListItems.Add(new ListItem_Me { id = 0, Name = "Мои АудиоЗаписи" });
            ListItems.Add(new ListItem_Me { id = 1, Name = "Рекомендации" });
            ListItems.Add(new ListItem_Me { id = 2, Name = "Популярное" });
        }
        public ObservableCollection<ListItem_Me> ListItems { get; set; }
    }
}
