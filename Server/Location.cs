using Meeting_Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Location {

        private readonly static String[] LOCATIONS = new String[5] { "PORTO", "LISBON", "FARO", "COIMBRA",
            "ÈVORA" };

        private Dictionary<String, List<Room>> rooms = new Dictionary<string, List<Room>>();

        public Dictionary<String, List<Room>> GetRooms {
            get{ return rooms; }
        }

        public Location()
        {
            this.rooms[LOCATIONS[0]] = new List<Room>();
            this.rooms[LOCATIONS[1]] = new List<Room>();
            this.rooms[LOCATIONS[2]] = new List<Room>();
            this.rooms[LOCATIONS[3]] = new List<Room>();
            this.rooms[LOCATIONS[4]] = new List<Room>();
        }

        //Adds room to dict based on location if room not in location
        public void addRoom(Room room, string location) {
            if (!LOCATIONS.Contains(location.ToUpper()))
            {
                throw new Exception("Can not add a room at this location as location does not exist");
            }
            foreach (KeyValuePair<string, List<Room>> entry in rooms) {
                if (entry.Key == location.ToUpper() && !entry.Value.Contains(room))
                {
                    rooms[entry.Key].Add(room);
                }               
            }
        }
    }
}
