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

        public void AddObserver(IStaffObserver observer)
        {
            // Check whether observer is already registered. If not, add it
            if (!observers.Contains(observer))
                observers.Add(observer);
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

        public Tempo GetMostRecentTempo()
        {
            throw new NotImplementedException();
        }

        public TimeSignature GetMostRecentTimeSignature()
        {
            throw new NotImplementedException();
        }

        public Staff()
        {
            Symbols = new List<StaffSymbol>();
        }
    }
}
