using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meeting_Calendar
{
    class Room
    {
        protected string name;
        protected int capacity;
        private List<DateTime> bookedDates = new List<DateTime>();

        public string Name {
            get { return name; }
            set { name = value; }
        }

        public int Capacity {
            get { return capacity; }
            set { capacity = value; }
        }

        public List<DateTime> BookedDates {
            get { return bookedDates; }
        }

        public Room(string name, int capacity)
        {
            this.capacity = capacity;
            this.name = name;
        }

        public void addBookedDate(DateTime date) {
            if (!bookedDates.Contains(date))
            {
                bookedDates.Add(date);
            }
            else
            {
                throw new Exception("Date is already booked for room " + this.name);
            }
        }

       
    }
}
