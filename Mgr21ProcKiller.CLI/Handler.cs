using ConsolePasswordMasker;
using CoreModel;
using CoreSecurityLib;
using CoreSecurityLib.Security;
using System;
using System.IO;
using CoreSecurityLib.Service;
using static System.Console;

namespace Mgr21ProcKiller.CLI
{
    public partial class Handler
    {
        private IOSecurity _iOSecurity = new IOSecurity();
        private LifetimeController _serviceController = new LifetimeController();
        public void Run()
        {
            MainResult serviceStarterResult = _serviceController.StartService();
            if (serviceStarterResult.Success)
            {
                Flags();
                WriteLine("\n  Press <enter> to continue...");
                ReadLine();
                Clear();
                if (!File.Exists(Global.MasterKeyLocation))
                {
                    SetUpMasterKey(new Action(HandleCommand));
                }
                else
                {
                    Login(new Action(HandleCommand));
                }
            }
            else
            {
                WriteLine($"Error: `{serviceStarterResult.ErrorMessage}`");
                WriteLine("Press <enter> to continue...");
                ReadLine();
            }
           
        }
        private void Flags()
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine("\n  ######################################");
            WriteLine("  ######################################");
            WriteLine("  ######################################");
            WriteLine("  ######################################");
            ForegroundColor = ConsoleColor.White;
            WriteLine("  ######################################");
            WriteLine("  ######################################");
            WriteLine("  ######################################");
            WriteLine("  ######################################\n");
            ForegroundColor = ConsoleColor.Green;
            WriteLine("  Mgr-21 PROCESS KILLER");
            WriteLine("  Created By: Mirza Ghulam Rasyid\n");
        }
        private void Menu()
        {
            ForegroundColor = ConsoleColor.Green;
            WriteLine("\n1. View black list");
            WriteLine("2. Block .exe files process");
            WriteLine("3. Remove .exe files black list");
            WriteLine("4. Clear black list");
            WriteLine("5. Settings");
            WriteLine("6. Change master password");
            WriteLine("7. Exit\n");
        }
        private void ChangePassword(Action action)
        {
            PasswordMasker masker = new PasswordMasker();
            string key = "";
            do
            {
                key = masker.Mask("Set password (length>5): ");
                if (String.IsNullOrEmpty(key) || key.Length <= 5)
                {
                    Write("Password length must be greater than 5 chars!");
                    ReadLine();
                    Clear();
                }
                else if (File.Exists(Global.MasterKeyLocation) && _iOSecurity.Login(key).Success)
                {
                    Write("You cannot input the same password!");
                    ReadLine();
                    Clear();
                }
                else
                    break;
            } while (true);
            WriteLine("Loading...");
            MainResult result = _iOSecurity.ChangeMasterKey(key);
            if (result.Success)
            {
                WriteLine("Password changed successfully");
            }
            else
                WriteLine($"Error: `{result.ErrorMessage}`");
            action();

        }
        private void SetUpMasterKey(Action action)
        {
            PasswordMasker masker = new PasswordMasker();
            string key = "";
            do
            {
                key = masker.Mask("Set password (length>5): ");
                if (String.IsNullOrEmpty(key) || key.Length <= 5)
                {
                    Write("Password length must be greater than 5 chars!");
                    ReadLine();
                    Clear();
                }
                else if (File.Exists(Global.MasterKeyLocation) && _iOSecurity.Login(key).Success)
                {
                    Write("You cannot input the same password!");
                    ReadLine();
                    Clear();
                }
                else
                    break;
            } while (true);
            WriteLine("Loading...");
            MainResult result = _iOSecurity.SetUpMasterKey(key);
            if (result.Success)
            {
                WriteLine("Password has been set");
                
            }
            else
                WriteLine($"Error: `{result.ErrorMessage}`");
            action();
        }
        private void Login(Action action)
        {
            int _loginCounter = 0;
            PasswordMasker masker = new PasswordMasker();
            MainResult result = new MainResult();
            string passCode = "";
            do
            {
                passCode = masker.Mask($"Login (attempt: {_loginCounter}/5): ");
                if (String.IsNullOrEmpty(passCode) || passCode.Length <= 5)
                {
                    Write("Password length must be greater than 5 chars!");
                    ReadLine();
                    Clear();
                }
                else
                {
                    result = _iOSecurity.Login(passCode);
                    if (result.Success)
                        break;
                    else
                    {
                        Write($"Error: `{result.ErrorMessage}`");
                        ReadLine();
                        Clear();
                    }
                }
                _loginCounter++;
            } while (_loginCounter <= 5);
            if (result.Success)
                action();
            else if (_loginCounter == 5)
                Environment.Exit(0);
        }
        private void HandleCommand()
        {
            while(true)
            {
                Clear();
                Menu();
                Write("Choose [1-7]> ");
                string input = ReadLine();
                int number=0;
                bool isNumber = int.TryParse(input, out number);
                while (!isNumber || (isNumber && (number<1 || number>7)))
                {
                    Write("Choose [1-7]> ");
                    input = ReadLine();
                    number = 0;
                    isNumber = int.TryParse(input, out number);
                }
                switch(number)
                {
                    case 1:
                        Command1();
                        break;
                    case 2:
                        Command2();
                        break;
                    case 3:
                        Command3();
                        break;
                    case 4:
                        Command4();
                        break;
                    case 5:
                        Command5();
                        break;
                    case 6:
                        Command6();
                        break;
                    default:
                        WriteLine("Press <enter> to exit...");
                        ReadLine();
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}
