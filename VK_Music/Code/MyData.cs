using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncrementalLoading.Code
{
    public class Item
    {
        public int id { get; set; }
        public string artist { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

    public class Response
    {
        public int count { get; set; }
        public List<Item> items { get; set; }
    }

    public class RootObject
    {
        public Response response { get; set; }
    }

    public class Audio
    {
        public int num { get; set; }
        public int id { get; set; }
        public int aid { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string artist { get; set; }
        public int duration { get; set; }
        public string duration_T { get; set; }
        public string image { get; set; }
        public string imageB { get; set; }
        public int lyrics_id { get; set; }
        public string owner_id { get; set; }
        public int getIntFromDatabase()
        { return lyrics_id; }
    }

    public class Group
    {
        public int Gid { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public int Lyrics_id { get; set; }
    }

    public class User
    {
        public int uid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string nickname { get; set; }
        public string photo_50 { get; set; }
        public string photo_100 { get; set; }

        public string Photo { get; set; }
        public string Name { get; set; }
        public int user_id { get; set; }
    }

    public class ListItem_Pop
    {
        public string Name { get; set; }
    }
}
