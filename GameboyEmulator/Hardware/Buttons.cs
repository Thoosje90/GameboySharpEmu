using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GameboyEmulator.Hardware
{

    class Keyboard
    {
        public int GetKeyMask(KeyEventArgs e)
        {
            int keyPressed = 0;

            if (e.KeyCode == Keys.Right)
            {
                keyPressed = 0x11;
            }
            else if (e.KeyCode == Keys.Left)
            {
                keyPressed = 0x12;
            }
            else if (e.KeyCode == Keys.Up)
            {
                keyPressed = 0x14;
            }
            else if (e.KeyCode == Keys.Down)
            {
                keyPressed = 0x18;
            }
            else if (e.KeyCode == Keys.Z)
            {
                keyPressed = 0x21;
            }
            else if (e.KeyCode == Keys.X)
            {
                keyPressed = 0x22;
            }
            else if (e.KeyCode == Keys.Space)
            {
                keyPressed = 0x24;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                keyPressed = 0x28;
            }


            return keyPressed;
        }


        #region Old Method


        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool GetKeyboardState(byte[] lpKeyState);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        //static extern short GetKeyState(int keyCode);

        //[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        //static extern short GetKeyState(int keyCode);

        //int curKeyPressed = 0;

        //public static bool ButtonPressed()
        //{
        //    return KeyPressed(Keys.Right) || KeyPressed(Keys.Left) || KeyPressed(Keys.Up) || KeyPressed(Keys.Down) || KeyPressed(Keys.Space) || KeyPressed(Keys.A) || KeyPressed(Keys.S) || KeyPressed(Keys.Return);
        //}

        //public int ListenKeyBoard()
        //{
        //    int keyPressed = 0;

        //    //case SDLK_a: key = 4; break;
        //    //case SDLK_s: key = 5; break;
        //    //case SDLK_RETURN: key = 7; break;
        //    //case SDLK_SPACE: key = 6; break;
        //    //case SDLK_RIGHT: key = 0; break;
        //    //case SDLK_LEFT: key = 1; break;
        //    //case SDLK_UP: key = 2; break;
        //    //case SDLK_DOWN: key = 3; break;

        //    if (KeyPressed(Keys.Right))
        //    {
        //        keyPressed = 0x11;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if (KeyPressed(Keys.Left))
        //    {
        //        keyPressed = 0x12;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.Up))
        //    {
        //        keyPressed = 0x14;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.Down))
        //    {
        //       keyPressed = 0x18;
        //       curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.A))
        //    {
        //        keyPressed = 0x21;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.S))
        //    {
        //        keyPressed = 0x22;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.Space))
        //    {
        //        keyPressed = 0x24;
        //        curKeyPressed = keyPressed;
        //    }
        //    else if(KeyPressed(Keys.Enter))
        //    {
        //        keyPressed = 0x28;
        //        curKeyPressed = keyPressed;
        //    }


        //    return keyPressed;
        //}

        //public int CheckReleased()
        //{
        //    int value = 0;

        //    if (curKeyPressed == 0x11 && !KeyPressed(Keys.Right))
        //        value = 0x11;
        //    if (curKeyPressed == 0x12 && !KeyPressed(Keys.Left))
        //        value = 0x12;
        //    if (curKeyPressed == 0x14 && !KeyPressed(Keys.Up))
        //        value = 0x14;
        //    if (curKeyPressed == 0x18 && !KeyPressed(Keys.Down))
        //        value = 0x18;
        //    if (curKeyPressed == 0x21 && !KeyPressed(Keys.A))
        //        value = 0x21;
        //    if (curKeyPressed == 0x22 && !KeyPressed(Keys.S))
        //        value = 0x22;
        //    if (curKeyPressed == 0x24 && !KeyPressed(Keys.Space))
        //        value = 0x24;
        //    if (curKeyPressed == 0x28 && !KeyPressed(Keys.Return))
        //        value = 0x28;

        //    if(value != 0)
        //    {
        //        curKeyPressed = 0;
        //    }

        //    return value;

        //}


        //public static byte GetVirtualKeyCode(Keys key)
        //{
        //    return (byte)((byte)key & 0xFF);
        //}

        //private static bool KeyPressed(Keys key)
        //{
        //    var code = GetVirtualKeyCode(key);
        //    short value = GetKeyState(code);

        //    if ((value & 0x8000) == 0x8000)
        //        return true;
        //    else
        //        return false;
        //}

        #endregion
    }
}
