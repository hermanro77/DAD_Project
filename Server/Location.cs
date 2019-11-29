using MeetingCalendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Location {

        private readonly static String[] LOCATIONS = new String[5] { "PORTO", "LISBOA", "FARO", "COIMBRA", "EVORA" };
        private Dictionary<String, List<Room>> rooms = new Dictionary<string, List<Room>>();
        //public Dictionary<String, List<Room>> GetRooms {
        //    get{ return rooms; }
        //}
        public List<Room> GetRooms(string location)
        {
            if (rooms.Keys.Contains(location.ToUpper())){
                return rooms[location.ToUpper()];
            } return null;
        }

        public Location()
        {
            this.rooms[LOCATIONS[0]] = new List<Room>();
            this.rooms[LOCATIONS[0]].Add(new Room("Snill pike", 8));
            this.rooms[LOCATIONS[0]].Add(new Room("Skitten ulv", 5));
            this.rooms[LOCATIONS[1]] = new List<Room>();
            this.rooms[LOCATIONS[1]].Add(new Room("To tette og en rødhette", 6));
            this.rooms[LOCATIONS[1]].Add(new Room("Tre bukk og en brus takk!", 10));
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
