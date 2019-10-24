//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Meeting_Calendar
//{
//    class Room
//    {
//        protected int capacity;
//        protected String room_name;
//        private List<String> booked_dates = new List<String>();

//        public Room(int cap, string name)
//        {
//            this.capacity = cap;
//            this.room_name = name;
//        }

//        public bool isAvailable(int date, int month, int year)
//        {
//            return booked_dates.Contains(
//                this.convertDate(date, month, year));
//        }

//        public void bookRoom(int date, int month, int year)
//        {
//            if (this.isAvailable(date, month, year))
//            {
//                booked_dates.add(this.convertDate(date, month, year));
//            }
//        }

//        private String convertDate(int date, int month, int year)
//        {
//            return (date.length == 1 ? "0" + date : date) + "/" + (month.length == 1 ? "0" + month : month) + "/" + year;
//        }
//    }
//}
