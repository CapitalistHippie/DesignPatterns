using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public class Staff
    {
        private List<IStaffObserver> observers = new List<IStaffObserver>();

        public int StaffNumber { get; set; }

        public string StaffName { get; set; }

        public string InstrumentName { get; set; }

        public List<StaffSymbol> Symbols { get; set; }

        public Staff()
        {
            Symbols = new List<StaffSymbol>();
        }

        public void AddObserver(IStaffObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void RemoveObserver(IStaffObserver observer)
        {
            observers.Remove(observer);
        }

        public void AddSymbol<T>(T staffSymbol) where T: StaffSymbol
        {
            Symbols.Add(staffSymbol);
            foreach (var observer in observers)
                observer.OnSymbolAdded(staffSymbol);
        }

        public StaffSymbol GetSymbol(int i)
        {
            return Symbols[i];
        }
    }
}
