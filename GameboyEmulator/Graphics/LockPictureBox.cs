using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    public class MyPictureBox : PictureBox
    {
        private object locker = new object();
        public new Image Image
        {
            get { return base.Image; }
            set { lock (locker) { base.Image = value; } }
        }
        public Image Clone()
        {
            lock (locker)
            {
                return (this.Image != null) ? (Image)this.Image.Clone() : null;
            }
        }
        protected override void OnPaint(PaintEventArgs pe)
        {
            lock (locker)
            {
                base.OnPaint(pe);
            }
        }
    }
}
