using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    public class DoubleControlBuffer
    {

        /// <summary>
        /// Sets the double buffered property of a list view to the specified value
        /// </summary>
        /// <param name="listView">The List view</param>
        /// <param name="doubleBuffered">Double Buffered or not</param>
        public static void SetDoubleBuffered(System.Windows.Forms.Control listView, bool doubleBuffered = true)
        {
            try
            {
                // Error handling
                if (listView == null)
                    throw new NullReferenceException("listView is null");

                // Hack doublebuffer property into control
                listView.GetType().GetProperty(
                    "DoubleBuffered", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                    )?.SetValue(listView, doubleBuffered, null);
            }
            catch (Exception) { }
        }

    }
}
