using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Mgr21ProcKiller.GUI
{
    public class LanguageResolver:Binding
    {
        public LanguageResolver(string name):base("[" + name + "]")
        {
            Mode = BindingMode.OneWay;
            Source = LanguageChanger.Instance;
        }
    }
}
