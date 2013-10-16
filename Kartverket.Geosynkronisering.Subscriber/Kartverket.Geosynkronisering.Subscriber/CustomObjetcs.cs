using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Kartverket.Geosynkronisering.Subscriber
{
    public class CueTextBox : TextBox
    {
        private static class NativeMethods
        {
            private const uint ECM_FIRST = 0x1500;
            internal const uint EM_SETCUEBANNER = ECM_FIRST + 1;

            [DllImport("user32.dll", EntryPoint = "SendMessageW")]
            public static extern IntPtr SendMessageW(IntPtr hWnd, UInt32 Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        }

        private string _cue;
        public string Cue
        {
            set
            {
                _cue = value;
                UpdateCue();
            }
        }

        private void UpdateCue()
        {
            if (IsHandleCreated && _cue != null)
            {
                NativeMethods.SendMessageW(Handle, NativeMethods.EM_SETCUEBANNER, (IntPtr)1, _cue);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateCue();
        }
    } 
}
