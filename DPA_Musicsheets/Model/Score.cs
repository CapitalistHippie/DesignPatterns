using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Score
    {
        private List<IScoreObserver> observers = new List<IScoreObserver>();

        private List<Staff> Staves { get; set; }

        public Score()
        {
            Staves = new List<Staff>();
        }

        public void AddObserver(IScoreObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void RemoveObserver(IScoreObserver observer)
        {
            observers.Remove(observer);
        }

        public void AddStaff(Staff staff)
        {
            Staves.Add(staff);
            foreach (var observer in observers)
                observer.OnStaffAdded(staff);
        }

        public Staff GetStaff(int index)
        {
            return Staves[index];
        }

        public int GetAmountOfStaves()
        {
            return Staves.Count;
        }
    }
}
