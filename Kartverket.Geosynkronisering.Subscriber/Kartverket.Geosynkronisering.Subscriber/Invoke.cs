using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Kartverket.Geosynkronisering.Subscriber
{
    public static class Invoke
    {
        public static void InvokeEx<T>(this T @this, Action<T> action) where T : ISynchronizeInvoke
        {
            try
            {
                if (@this.InvokeRequired)
                {
                    @this.Invoke(action, new object[] {@this});
                }
                else
                {
                    action(@this);
                }
            }catch (Exception ex)
            {}
        }
    }
}
