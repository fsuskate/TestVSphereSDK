using System;
using System.Linq;
using VMware.Vim;
using System.Collections.Specialized;

namespace TestVSphereSDK
{
    public static class MyExtensions
    {
        public static T Find<T>(this VimClient vimClient, string searchKey, string searchValue)
        {
            NameValueCollection filter = new NameValueCollection();
            filter.Add(searchKey, "^" + searchValue + "$");
            return (T)Convert.ChangeType(vimClient.FindEntityView(typeof(T), null, filter, null), typeof(T));
        }

        public static L Find<L, T>(this VimClient vimClient, string searchKey, string searchValue)
        {
            NameValueCollection filter = new NameValueCollection();
            filter.Add(searchKey, "^" + searchValue + "$");
            return (L)Convert.ChangeType(vimClient.FindEntityViews(typeof(T), null, filter, null), typeof(T));
        }

        public static T FindByMoRef<T>(this VimClient vimClient, string value)
        {
            string typeString = typeof(T).ToString().Split('.').Last<string>();
            ManagedObjectReference moref = new ManagedObjectReference();
            moref.Type = "ManagedObjectReference:" + typeString;
            moref.Value = value;
            return (T)Convert.ChangeType(vimClient.FindEntityView(typeof(VirtualMachine), moref, null, null), typeof(T));
        }
    }
}
