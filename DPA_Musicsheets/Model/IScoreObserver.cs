using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPA_Musicsheets.Model
{
    public interface IScoreObserver
    {
        void OnStaffAdded(Staff staff);
    }
}
