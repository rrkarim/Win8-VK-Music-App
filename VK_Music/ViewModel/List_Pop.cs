using IncrementalLoading.Code;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VK_Music.ViewModel
{
    public class Me_ListViewModel_Pop
    {
        public Me_ListViewModel_Pop()
        {
            ListItems_Pop = new ObservableCollection<ListItem_Pop>();
            ListItems_Pop.Add(new ListItem_Pop { Name = "Мои Аудиозаписи" });
            ListItems_Pop.Add(new ListItem_Pop { Name = "Аудиозаписи друзей" });
            ListItems_Pop.Add(new ListItem_Pop { Name = "Аудиозаписи групп" });
        }
        public ObservableCollection<ListItem_Pop> ListItems_Pop { get; set; }
    }
}
