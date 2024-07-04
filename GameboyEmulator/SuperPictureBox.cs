using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameboyEmulator
{
    public class PictureBoxWithInterpolationMode : PictureBox
    {
        public InterpolationMode InterpolationMode { get; set; }

        Stopwatch lol = new Stopwatch();
        int targetMillis, lastTime, targetTime;
        public static float timeScaler = 1;

        public PictureBoxWithInterpolationMode()
        {
            //this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            //if (!lol.IsRunning)
            //{
            //    lol.Start();

            //    targetMillis = 1000 / 60;
            //    lastTime = (int)lol.ElapsedMilliseconds;
            //    targetTime = lastTime + targetMillis;

            //}

            //int current = (int)lol.ElapsedMilliseconds; // Now time
            //if (current < targetTime)
            //    return; // Stop here if its not time for the next frame

            //timeScaler = (float)targetMillis / (current - lastTime);
            ////Scale game on how late frame is.
            //lastTime = current;
            //targetTime = (current + targetMillis) - (current - targetTime);


            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }
}
