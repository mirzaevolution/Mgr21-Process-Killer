using CoreModel;
using Newtonsoft.Json;
using System;
using System.IO;
namespace CoreSecurityLib.Common
{

    public class Config
    {
        public static MainResult StoreSettings(Settings settings)
        {
            bool success = true;
            string message = "";
            try
            {
                string json = JsonConvert.SerializeObject(settings);
                if(String.IsNullOrEmpty(json))
                {
                    success = false;
                    message = "An error occured during settings serialization";
                }
                else
                {
                    File.WriteAllText(Global.SettingFileLocation, json);
                }
            }
            catch(Exception ex)
            {
                success = false;
                message = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = message
            };
        }
        public static DataResult<Settings> RetrieveSettings()
        {
            bool success = true;
            string message = "";
            if (!File.Exists(Global.SettingFileLocation))
            {
                return new DataResult<Settings>
                {
                    Data = new Settings
                    {
                        Interval = 1000,
                        ComparisonType = ComparisonType.ByName
                    },
                    MainResult = new MainResult
                    {
                        Success = success,
                        ErrorMessage = message
                    }
                };
            }
            else
            {
                Settings settings = null;
                try
                {
                    string dataText = File.ReadAllText(Global.SettingFileLocation);
                    if(String.IsNullOrEmpty(dataText))
                    {
                        success = false;
                        message = "Error reading setting file";
                    }
                    else
                    {
                        settings =  JsonConvert.DeserializeObject<Settings>(dataText);
                    }
                }
                catch (Exception ex)
                {
                    settings = settings != null ? null : settings;
                    success = false;
                    message = ex.Message;
                }
                return new DataResult<Settings>
                {
                    Data = settings,
                    MainResult = new MainResult
                    {
                        Success = success,
                        ErrorMessage = message
                    }
                };
            }
        }
    }
}
