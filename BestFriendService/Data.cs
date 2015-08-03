using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BestFriendService
{
    public sealed class Data
    {
        private static IList<City> _cities;

        public static IList<City> Cities
        {
            get
            {
                if (_cities == null)
                {
                    _cities = new List<City>();
                    _cities.Add(new City() { Name = "London",       Image = "ms-appx:///Assets/city/london.jpg" });
                    _cities.Add(new City() { Name = "New York",     Image = "ms-appx:///Assets/city/newyork.jpg" });
                    _cities.Add(new City() { Name = "Moscow",       Image = "ms-appx:///Assets/city/moscow.jpg" });
                }
                return _cities;
            }
        }

    }
}
