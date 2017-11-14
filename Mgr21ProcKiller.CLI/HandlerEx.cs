using CoreModel;
using CoreSecurityLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static System.Console;
namespace Mgr21ProcKiller.CLI
{
    public partial class  Handler
    {
        private void Command1()
        {

            ViewList();
            WriteLine("\nPress <enter> to continue...");
            ReadLine();
            Clear();
        }
        private void Command2()
        {
            WriteLine("Note: Duplicate items won't be added to black list");
            WriteLine("Press <enter> to add item");
            ReadLine();
            WriteLine("Retrieving list....");
            DataResult<List<ProcessModel>> dataResult = _iOSecurity.RetrieveData();
            if (dataResult.MainResult.Success)
            {
                List<ProcessModel> list = dataResult.Data;
                OpenFileDialog fileDialog = new OpenFileDialog()
                {
                    Filter = "Executable File (*.exe)|*.exe",
                    FileName = "",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = true
                };
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    int added = 0;
                    foreach (string name in fileDialog.FileNames)
                    {
                        if (!list.Any(x => x.FileLocation.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            DataResult<ProcessModel> addResult = _iOSecurity.CreateProcessModel(name);
                            if (addResult.MainResult.Success)
                            {
                                list.Add(addResult.Data);
                                added++;
                            }
                            else
                            {
                                WriteLine($"Error: `{addResult.MainResult.ErrorMessage}`");
                            }
                        }
                    }
                    if (added > 0)
                    {
                        WriteLine("Adding data....");
                        MainResult storeResult = _iOSecurity.StoreData(list);
                        if (storeResult.Success)
                        {
                            WriteLine($"\n{added} item(s) added to black list");
                            ViewList();

                            //refresing service.....
                            WriteLine($"Refreshing service...");
                            MainResult refreshResult = _serviceController.RefreshService();
                            if (refreshResult.Success)
                                WriteLine("Service refreshed");
                            else
                                WriteLine(refreshResult.ErrorMessage);
                            //......................
                        }
                        else
                        {
                            WriteLine($"Error: `{storeResult.ErrorMessage}`");
                        }
                    }
                    else
                        WriteLine("No item(s) beign added");
                }
                else
                {
                    WriteLine("Operation cancelled");
                }
            }
            else
            {
                WriteLine($"Error: `{dataResult.MainResult.ErrorMessage}`");
            }
            WriteLine("\nPress <enter> to continue...");
            ReadLine();
            Clear();
        }
        private void Command3()
        {
            List<ProcessModel> list = ViewList();
            if (list != null)
            {
                if (list.Count > 0)
                {
                    Write($"Choose [1-{list.Count}]> ");
                    string input = ReadLine();
                    int number = 0;
                    bool isNumber = int.TryParse(input, out number);
                    while (!isNumber || (isNumber && (number < 1 || number > list.Count)))
                    {
                        Write($"Choose [1-{list.Count}]> ");
                        input = ReadLine();
                        number = 0;
                        isNumber = int.TryParse(input, out number);
                    }
                    bool success = true;
                    try
                    {
                        list.RemoveAt(number - 1);
                    }
                    catch { success = false; }
                    if (success)
                    {
                        WriteLine("Removing item...");
                        MainResult deleteResult = _iOSecurity.StoreData(list);
                        if (deleteResult.Success)
                        {
                            WriteLine("Item removed");
                            WriteLine("Viewing current list...");
                            ViewList();
                            //refresing service.....
                            WriteLine($"Refreshing service...");
                            MainResult refreshResult = _serviceController.RefreshService();
                            if (refreshResult.Success)
                                WriteLine("Service refreshed");
                            else
                                WriteLine(refreshResult.ErrorMessage);
                            //......................
                        }
                        else
                            WriteLine($"Error: `{deleteResult.ErrorMessage}`");
                    }
                    else
                        WriteLine("Failed to remove selected item");
                }
                else
                    WriteLine("Nothing to remove");
            }
            WriteLine("\nPress <enter> to continue...");
            ReadLine();
            Clear();
        }
        private void Command4()
        {
            ViewList();
            Write("Are you sure want to clear black list? [Y/N]> ");
            string confirm = ReadLine();
            while ((!confirm.Equals("y", StringComparison.InvariantCultureIgnoreCase) &&
                !confirm.Equals("n", StringComparison.InvariantCultureIgnoreCase)))
            {
                Write("Are you sure want to clear black list? [Y/N]> ");
                confirm = ReadLine();
            }
            switch (confirm.ToLower())
            {
                case "y":
                    WriteLine("Clearing the list...");
                    var clearResult = _iOSecurity.StoreData(new List<ProcessModel>());
                    if (clearResult.Success)
                    {
                        WriteLine("black list has been cleared");
                        //refresing service.....
                        WriteLine($"Refreshing service...");
                        MainResult refreshResult = _serviceController.RefreshService();
                        if (refreshResult.Success)
                            WriteLine("Service refreshed");
                        else
                            WriteLine(refreshResult.ErrorMessage);
                        //......................
                    }
                    else
                        WriteLine("Failed to clear the list due to unexcepted error. Please try again");
                    break;
                default:
                    WriteLine("Operation cancelled");
                    break;
            }
            WriteLine("\nPress <enter> to continue...");
            ReadLine();
            Clear();
        }
        private void Command5()
        {
            WriteLine("Retrieving current settings...");
            DataResult<Settings> dataResult = Config.RetrieveSettings();
            if(dataResult.MainResult.Success)
            {
                WriteLine(">> Settings\n");
                WriteLine("1. Set the comparison type");
                WriteLine($"   Current-Setting: `{dataResult.Data.ComparisonType.ToString()}`");
                WriteLine("2. Set timer interval in milliseconds");
                WriteLine($"   Current-Setting: `{dataResult.Data.Interval} ms`");
                WriteLine("3. Exit to main menu\n");
                Write("Choose [1-3]> ");
                string input = ReadLine();
                int number = 0;
                bool isNumber = int.TryParse(input, out number);
                while(!isNumber || (isNumber && (number<1 || number>3)))
                {
                    Write("Choose [1-3]> ");
                    input = ReadLine();
                    number = 0;
                    isNumber = int.TryParse(input, out number);
                }
                switch(number)
                {
                    case 1:
                        ChangeComparisonType();
                        break;
                    case 2:
                        ChangeInterval();
                        break;
                };
            }
            else
            {
                WriteLine($"Error: `{dataResult.MainResult.ErrorMessage}`");
            }
            WriteLine("\nPress <enter> to continue...");
            ReadLine();
            Clear();
        }
        private void Command6()
        {
            Login(new Action(() => ChangePassword(new Action(() =>
            {
                WriteLine("Press <enter> to continue...");
                ReadLine();
                Clear();
            }))));
        }
        private List<ProcessModel> ViewList()
        {

            WriteLine("Loading...");
            DataResult<List<ProcessModel>> dataResult = _iOSecurity.RetrieveData();
            List<ProcessModel> list = null;
            if (dataResult.MainResult.Success)
            {

                if (dataResult.Data.Count > 0)
                {
                    int index = 1;
                    list = dataResult.Data;
                    foreach (ProcessModel model in list)
                    {
                        WriteLine($"Data #{index}: ");
                        WriteLine($"\tProcess Name: {model.ProcessName}");
                        WriteLine($"\tLocation: {model.FileLocation}");
                        WriteLine($"\tSize: {model.FileLength} bytes\n");
                        index++;
                    }
                }
                else
                {
                    list = new List<ProcessModel>();
                    WriteLine("No item in black list");
                }
            }
            else
            {
                WriteLine($"Error: `{dataResult.MainResult.ErrorMessage}`");
            }
            return list;
        }
        private void ChangeComparisonType()
        {
            DataResult<Settings> dataResult = Config.RetrieveSettings();
            if(dataResult.MainResult.Success)
            {
                ComparisonType type = dataResult.Data.ComparisonType;
                WriteLine($"Current Setting For Comparison Type: {type.ToString()}");
                string name = Enum.GetNames(typeof(ComparisonType)).Where(x => !x.Equals(type.ToString(), StringComparison.InvariantCultureIgnoreCase)).First();
                ComparisonType nextType =(ComparisonType)Enum.Parse(typeof(ComparisonType),name);
                Write($"Do you want to change comparison type to `{name}` ? [Y|N]> ");
               
                string confirm = ReadLine();
                while ((!confirm.Equals("y", StringComparison.InvariantCultureIgnoreCase) &&
                    !confirm.Equals("n", StringComparison.InvariantCultureIgnoreCase)))
                {
                    Write($"Do you want to change comparison type to `{name}` ? [Y|N]> ");
                    confirm = ReadLine();
                }
                switch(confirm.ToLower())
                {
                    case "y":
                        Settings settings = dataResult.Data;
                        settings.ComparisonType = nextType;
                        WriteLine("Updating settings...");
                        MainResult result = Config.StoreSettings(settings);
                        if (result.Success)
                        {
                            WriteLine("Comparison type has been changed");
                            //refresing service.....
                            WriteLine($"Refreshing service...");
                            MainResult refreshResult = _serviceController.RefreshService();
                            if (refreshResult.Success)
                                WriteLine("Service refreshed");
                            else
                                WriteLine(refreshResult.ErrorMessage);
                            //......................
                        }
                        else
                            WriteLine($"Error: `{result.ErrorMessage}`");
                        break;
                    default:
                        WriteLine("Operation cancelled");
                        break;
                }
            }

            else
            {
                WriteLine($"Error: `{dataResult.MainResult.ErrorMessage}`");
            }
        }
        private void ChangeInterval()
        {
            DataResult<Settings> dataResult = Config.RetrieveSettings();
            if (dataResult.MainResult.Success)
            {
                
                WriteLine($"Current Setting For Interval: {dataResult.Data.Interval} ms");
                Write("Input new interval [min: 500, cancel: -1]> ");
                string input = ReadLine();
                int number = 0;
                bool isNumber = int.TryParse(input, out number);
                while (!isNumber || number < 500)
                {
                    if (number == -1)
                        break;
                    Write("Input new interval [min:500, cancel :-1]> ");
                    input = ReadLine();
                    number = 0;
                    isNumber = int.TryParse(input, out number);
                }
                if (number != -1)
                {
                    Settings settings = dataResult.Data;
                    settings.Interval = number;
                    WriteLine("Updating settings...");
                    MainResult result = Config.StoreSettings(settings);
                    if (result.Success)
                    {
                        WriteLine($"Timer interval has been changed to {number} ms");
                        //refresing service.....
                        WriteLine($"Refreshing service...");
                        MainResult refreshResult = _serviceController.RefreshService();
                        if (refreshResult.Success)
                            WriteLine("Service refreshed");
                        else
                            WriteLine(refreshResult.ErrorMessage);
                        //......................
                    }
                    else
                        WriteLine($"Error: `{result.ErrorMessage}`");
                }
                else
                    WriteLine("Operation cancelled");
            }

            else
            {
                WriteLine($"Error: `{dataResult.MainResult.ErrorMessage}`");
            }
        }
    }
}
