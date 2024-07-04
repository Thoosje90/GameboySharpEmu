using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator.Events
{
    public delegate void InteruptEventHandler(object sender, InteruptRequestedEventArgs e);

    public class InteruptRequestedEventArgs : EventArgs
    {
        public InteruptRequestedEventArgs(int id)
        {
            this.InteruptID = id;
        }

        public int InteruptID { get; private set; }

    }
}
