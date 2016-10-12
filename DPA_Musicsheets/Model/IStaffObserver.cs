using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public interface IStaffObserver
    {
        void OnSymbolAdded<T>(T symbol) where T : StaffSymbol;
    }
}
