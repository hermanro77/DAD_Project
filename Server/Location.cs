using Meeting_Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Location {

        private readonly static String[] locations = new String[5] { "PORTO", "LISBON", "FARO", "COIMBRA",
            "ÈVORA" };

        private Dictionary<String, List<Room>> rooms = new Dictionary<string, List<Room>>();

        //Adds room to dict based on location if room not in location
        public void addRoom(Room room, string location) {
            if (!locations.Contains(location.ToUpper()))
            {
                throw new Exception("Can not add a room at this location as location does not exist");
            }
            foreach (KeyValuePair<string, List<Room>> entry in rooms) {
                if (entry.Key == location && !entry.Value.Contains(room))
                {
                    entry.Value.Add(room);
                }               
            }
        }



    }
}
