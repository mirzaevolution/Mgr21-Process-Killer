using System;
using System.IO;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Cryptography;
using Newtonsoft.Json;
using MirzaCryptoHelpers.Hashings;
using MirzaCryptoHelpers.Common;
using MirzaCryptoHelpers.SymmetricCryptos;
using CoreModel;

namespace CoreSecurityLib.Security
{
    public class IOSecurity
    {
        private MainResult LockFile(string filename)
        {
            if (String.IsNullOrEmpty(filename))
            {
                return new MainResult
                {
                    ErrorMessage = "File location cannot be null",
                    Success = false
                };
            }
            bool success = true;
            string error = "";
            try
            {
                if (!File.Exists(filename))
                {
                    success = false;
                    error = $"File `{filename}` is not found";
                }
                else
                {
                    FileSecurity fileSecurity = File.GetAccessControl(filename, AccessControlSections.Access);
                    var accessRules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
                    foreach (FileSystemAccessRule accessRule in accessRules)
                    {
                        if (accessRule.AccessControlType == AccessControlType.Deny)
                        {
                            bool result = fileSecurity.RemoveAccessRule(accessRule);
                            if (result)
                            {
                                File.SetAccessControl(filename, fileSecurity);
                            }
                        }
                    }
                    fileSecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Deny));
                    File.SetAccessControl(filename, fileSecurity);
                }
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }

        private MainResult UnlockFile(string filename, bool isNew=false)
        {
            if (String.IsNullOrEmpty(filename))
            {
                return new MainResult
                {
                    ErrorMessage = "File location cannot be null",
                    Success = false
                };
            }
            
            bool success = true;
            string error = "";
            if (isNew)
                return new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                };
            try
            {
                if (!File.Exists(filename))
                {
                    success = false;
                    error = $"File `{filename}` is not found";
                }
                else
                {
                    FileSecurity fileSecurity = File.GetAccessControl(filename, AccessControlSections.Access);
                    var accessRules = fileSecurity.GetAccessRules(true, true, typeof(NTAccount));
                    foreach (FileSystemAccessRule accessRule in accessRules)
                    {
                        bool result = fileSecurity.RemoveAccessRule(accessRule);
                        if (result)
                        {
                            File.SetAccessControl(filename, fileSecurity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }

        private MainResult StoreMasterKey(string clearTextPassword)
        {
            if (String.IsNullOrEmpty(clearTextPassword))
            {
                return new MainResult
                {
                    ErrorMessage = "Password cannot be null",
                    Success = false
                };
            }
            bool success = true;
            string error = "";
            try
            {
                byte[] hashedKey = new SHA256Crypto().GetHashBytes(clearTextPassword);
                if(hashedKey==null)
                {
                    success = false;
                    error = "Failed to hash master key";
                }
                else
                {
                    byte[] encryptedKey = null;
                    try
                    {
                        //this entropy is not a secret
                        byte[] optionalEntropy = BitHelpers.CreateSecurePassword("#EF$RCVDFQER!#@#~~!WE@R@?>?KU$%#THT%#$HK%TH%YU%**($%DADGDFHWEJdfsffwgt34t#%@#r4t3T3r43",128);
                        encryptedKey = ProtectedData.Protect(hashedKey, optionalEntropy, DataProtectionScope.LocalMachine);
                    }
                    catch
                    {
                        encryptedKey = null;
                    }
                    if (encryptedKey == null)
                    {
                        success = false;
                        error = "Failed during master key encryption";
                    }
                    else
                    {
                        bool exists = File.Exists(Global.MasterKeyLocation) ? true : false;
                        MainResult unlockFileResult = null;

                        unlockFileResult = exists ? UnlockFile(Global.MasterKeyLocation) : new MainResult();

                        if (exists && !unlockFileResult.Success)
                        {
                            success = false;
                            error = unlockFileResult.ErrorMessage;
                        }
                        else
                        {
                            bool save = true;
                            try
                            {
                                File.WriteAllBytes(Global.MasterKeyLocation, encryptedKey);
                            }
                            catch { save = false; }
                            if (!save)
                            {
                                success = false;
                                error = "Failed to write master key to desired location";
                            }
                            else
                            {
                                var result = LockFile(Global.MasterKeyLocation);
                                if (!result.Success)
                                {
                                    success = false;
                                    error = result.ErrorMessage;
                                }
                            }
                        }
                       
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }

        private DataResult<string> RetrieveHashedMasterKeyBase64()
        {
            bool success = true;
            string error = "";
            string resultBase64Str = "";
            try
            {
                var unlockResult = UnlockFile(Global.MasterKeyLocation);
                if(!unlockResult.Success)
                {
                    success = false;
                    error = unlockResult.ErrorMessage;
                }
                else
                {
                    byte[] encryptedMasterKey = null;

                    using(FileStream fs = new FileStream(Global.MasterKeyLocation,FileMode.Open,FileAccess.Read,FileShare.Read))
                    {
                        try
                        {
                            encryptedMasterKey = new byte[fs.Length];
                            fs.Read(encryptedMasterKey, 0, encryptedMasterKey.Length);
                        }
                        catch { }
                    }

                    if(encryptedMasterKey == null)
                    {
                        success = false;
                        error = "Error reading master key file";
                    }
                    else
                    {
                        //lock master key again
                        var lockResult = LockFile(Global.MasterKeyLocation);
                        if(!lockResult.Success)
                        {
                            success = false;
                            error = lockResult.ErrorMessage;
                        }
                        else
                        {
                            byte[] decryptedMasterKey = null;
                            try
                            {
                                //this entropy is not a secret
                                byte[] optionalEntropy = BitHelpers.CreateSecurePassword("#EF$RCVDFQER!#@#~~!WE@R@?>?KU$%#THT%#$HK%TH%YU%**($%DADGDFHWEJdfsffwgt34t#%@#r4t3T3r43", 128);
                                decryptedMasterKey = ProtectedData.Unprotect(encryptedMasterKey, optionalEntropy, DataProtectionScope.LocalMachine);
                            }
                            catch
                            {
                                decryptedMasterKey = null;
                            }
                            if(decryptedMasterKey==null)
                            {
                                success = false;
                                error = "Error during master key decryption";
                            }
                            else
                            {
                                resultBase64Str = BitHelpers.ConvertToBase64String(decryptedMasterKey);
                                if(String.IsNullOrEmpty(resultBase64Str))
                                {
                                    success = false;
                                    error = "Error converting decrypted master key to base64 string";
                                }

                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new DataResult<string>
            {
                MainResult = new MainResult
                {
                    ErrorMessage = error,
                    Success = success
                },
                Data = resultBase64Str
            };

        }

        private DataResult<byte[]> RetrieveHashedMasterKeyBytes()
        {
            bool success = true;
            string error = "";
            byte[] result = null;
            try
            {
                var unlockResult = UnlockFile(Global.MasterKeyLocation);
                if (!unlockResult.Success)
                {
                    success = false;
                    error = unlockResult.ErrorMessage;
                }
                else
                {
                    byte[] encryptedMasterKey = null;

                    using (FileStream fs = new FileStream(Global.MasterKeyLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        try
                        {
                            encryptedMasterKey = new byte[fs.Length];
                            fs.Read(encryptedMasterKey, 0, encryptedMasterKey.Length);
                        }
                        catch { }
                    }
                    if (encryptedMasterKey == null)
                    {
                        success = false;
                        error = "Error reading master key file";
                    }
                    else
                    {
                        //lock master key again
                        var lockResult = LockFile(Global.MasterKeyLocation);
                        if (!lockResult.Success)
                        {
                            success = false;
                            error = lockResult.ErrorMessage;
                        }
                        else
                        {
                            byte[] decryptedMasterKey = null;
                            try
                            {
                                //this entropy is not a secret
                                byte[] optionalEntropy = BitHelpers.CreateSecurePassword("#EF$RCVDFQER!#@#~~!WE@R@?>?KU$%#THT%#$HK%TH%YU%**($%DADGDFHWEJdfsffwgt34t#%@#r4t3T3r43", 128);
                                decryptedMasterKey = ProtectedData.Unprotect(encryptedMasterKey, optionalEntropy, DataProtectionScope.LocalMachine);
                            }
                            catch
                            {
                                decryptedMasterKey = null;
                            }
                            if (decryptedMasterKey == null)
                            {
                                success = false;
                                error = "Error during master key decryption";
                            }
                            else
                            {
                                result = decryptedMasterKey;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new DataResult<byte[]>
            {
                MainResult = new MainResult
                {
                    ErrorMessage = error,
                    Success = success
                },
                Data = result
            };

        }

        private DataResult<string> SerializeData(List<ProcessModel> list)
        {
            if (list == null)
                return new DataResult<string>
                {
                    Data = null,
                    MainResult = new MainResult
                    {
                        ErrorMessage = "List cannot be null",
                        Success = false
                    }
                };
            bool success = true;
            string error = "";
            string serializedJson = "";
            try
            {
                serializedJson = JsonConvert.SerializeObject(list);
            }
            catch(Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new DataResult<string>
            {
                Data = serializedJson,
                MainResult = new MainResult
                {
                    ErrorMessage = error,
                    Success = success
                }
            };
        }

        private DataResult<List<ProcessModel>> DeserializeData(string jsonData)
        {
            bool success = true;
            string error = "";
            List<ProcessModel> list = null;
            try
            {
                list = JsonConvert.DeserializeObject<List<ProcessModel>>(jsonData);
                
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new DataResult<List<ProcessModel>>
            {
                Data = list,
                MainResult = new MainResult
                {
                    ErrorMessage = error,
                    Success = success
                }
            };
        }

        public MainResult SetUpMasterKey(string clearTextPassword)
        {
            if (String.IsNullOrEmpty(clearTextPassword))
                return new MainResult
                {
                    Success = false,
                    ErrorMessage = "Password cannot be null"
                };
            return StoreMasterKey(clearTextPassword);
        }

        public MainResult Login(string clearTextPassword)
        {
            if(String.IsNullOrEmpty(clearTextPassword))
            {
                return new MainResult
                {
                    ErrorMessage = "Password cannot be null",
                    Success = false
                };
            }
            
            bool success = true;
            string error = "";
            try
            {
                var hashedMasterKeyResult = RetrieveHashedMasterKeyBytes();
                if(!hashedMasterKeyResult.MainResult.Success)
                {
                    success = false;
                    error = hashedMasterKeyResult.MainResult.ErrorMessage;
                }
                else
                {
                    byte[] hashedMasterKey = hashedMasterKeyResult.Data;
                    byte[] hashedPassword = new SHA256Crypto().GetHashBytes(clearTextPassword);
                    if(hashedPassword == null)
                    {
                        success = false;
                        error = "An error occured during password hashing";
                    }
                    else
                    {
                        if(!BitComparer.CompareBytes(hashedMasterKey,hashedPassword))
                        {
                            success = false;
                            error = "Login failed. Password is invalid";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }
        
        public MainResult StoreData(List<ProcessModel> list)
        {
            if (list == null)
                return new MainResult
                {
                    Success = false,
                    ErrorMessage = "List is null"
                };
            bool success = true;
            string error = "";
            try
            {
       
                //it'll be used for the initial store.
                if (!File.Exists(Global.DataFileLocation))
                {
                    File.WriteAllText(Global.DataFileLocation, null);
                }


                //if zero, it doesn't need to be encrypted
                if(list.Count == 0)
                {

                    var unlockResult = UnlockFile(Global.DataFileLocation);
                    if(!unlockResult.Success)
                    {
                        success = false;
                        error = unlockResult.ErrorMessage;
                    }
                    else
                    {
                        bool save = true;
                        try
                        {
                            File.WriteAllText(Global.DataFileLocation, null);
                        }
                        catch { save = false; }
                        if(!save)
                        {
                            success = false;
                            error = "An error occured while saving data";
                        }
                        else
                        {
                            var lockResult = LockFile(Global.DataFileLocation);
                            if (!lockResult.Success)
                            {
                                success = false;
                                error = lockResult.ErrorMessage;
                            }
                        }
                    }
                }
                else
                {
                    var serializationResult = SerializeData(list);
                    if(!serializationResult.MainResult.Success)
                    {
                        success = false;
                        error = serializationResult.MainResult.ErrorMessage;
                    }
                    else
                    {
                        string serializedJson = serializationResult.Data;

                        var masterKeyResult = RetrieveHashedMasterKeyBase64();
                        if(!masterKeyResult.MainResult.Success)
                        {
                            success = false;
                            error = masterKeyResult.MainResult.ErrorMessage;
                        }
                        else
                        {
                            string masterKeyBase64 = masterKeyResult.Data;
                            byte[] encryptedData = new AESCrypto().Encrypt(BitHelpers.ConvertStringToBytes(serializedJson), masterKeyBase64);
                            if(encryptedData==null)
                            {
                                success = false;
                                error = "An error occured during data encryption";
                            }
                            else
                            {
                                var unlockDataResult = UnlockFile(Global.DataFileLocation);
                                if (!unlockDataResult.Success)
                                {
                                    success = false;
                                    error = unlockDataResult.ErrorMessage;
                                }
                                else
                                {
                                    bool save = true;
                                    try
                                    {
                                        File.WriteAllBytes(Global.DataFileLocation, encryptedData);
                                    }
                                    catch { save = false; }
                                    if (!save)
                                    {
                                        success = false;
                                        error = "An error occured while saving data";
                                    }
                                    var lockResult = LockFile(Global.DataFileLocation);
                                    if (!lockResult.Success)
                                    {
                                        success = false;
                                        error = lockResult.ErrorMessage;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }

        public DataResult<List<ProcessModel>> RetrieveData()
        {
            bool success = true;
            string error = "";
            List<ProcessModel> list = null;
            try
            {
                var masterKeyResult = RetrieveHashedMasterKeyBase64();
                if(!masterKeyResult.MainResult.Success)
                {
                    success = false;
                    error = masterKeyResult.MainResult.ErrorMessage;
                }
                else
                {
                    string masterKeyBase64 = masterKeyResult.Data;
                    //1st time loading just returns empty list
                    if(!File.Exists(Global.DataFileLocation))
                    {
                        list = new List<ProcessModel>();
                    }
                    else
                    {
                        var unlockDataFileResult = UnlockFile(Global.DataFileLocation);
                        if(!unlockDataFileResult.Success)
                        {
                            success = false;
                            error = unlockDataFileResult.ErrorMessage;
                        }
                        else
                        {
                            byte[] encryptedDataBytes = null;
                            try
                            {
                                encryptedDataBytes = File.ReadAllBytes(Global.DataFileLocation);
                            }
                            catch { }

                            LockFile(Global.DataFileLocation);

                            if(encryptedDataBytes==null)
                            {
                                success = false;
                                error = "Error while reading data";
                            }
                            //if data is empty, just return empty list
                            else if(encryptedDataBytes.Length == 0)
                            {
                                list = new List<ProcessModel>();
                            }
                            else
                            {
                                byte[] decryptedDataBytes = new AESCrypto().Decrypt(encryptedDataBytes, masterKeyBase64);
                                if(decryptedDataBytes == null)
                                {
                                    success = false;
                                    error = "Error while decrypting data";
                                }
                                else
                                {
                                    var deserializationResult = DeserializeData(BitHelpers.ConvertBytesToString(decryptedDataBytes));
                                    if(!deserializationResult.MainResult.Success)
                                    {
                                        success = false;
                                        error = deserializationResult.MainResult.ErrorMessage;
                                    }
                                    else
                                    {
                                        list = deserializationResult.Data;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                error = ex.Message;
            }
            return new DataResult<List<ProcessModel>>
            {
                Data = list,
                MainResult = new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                }
            };
        }

        public DataResult<ProcessModel> CreateProcessModel(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                return new DataResult<ProcessModel>
                {
                    Data = null,
                    MainResult = new MainResult
                    {
                        Success = false,
                        ErrorMessage = "Filename cannot be null"
                    }
                };
            if (!File.Exists(filename))
                return new DataResult<ProcessModel>
                {
                    Data = null,
                    MainResult = new MainResult
                    {
                        Success = false,
                        ErrorMessage = "File doesn't exist"
                    }
                };

            bool success = true;
            string error = "";
            ProcessModel processModel = null;
            try
            {
                FileInfo fileInfo = new FileInfo(filename);
                byte[] buffer = new byte[2048];
                using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < buffer.Length)
                        buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                }
                processModel = new ProcessModel
                {
                    FileLocation = filename,
                    ProcessName = Path.GetFileNameWithoutExtension(fileInfo.Name),
                    FileLength = fileInfo.Length,
                    InitialBytes = buffer
                };

            }
            catch (Exception ex)
            {
                processModel = null;
                success = false;
                error = ex.Message;
            }

            return new DataResult<ProcessModel>
            {
                Data = processModel,
                MainResult = new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                }
            };
        }
        
        public MainResult ChangeMasterKey(string clearTextPassword)
        {
            if (String.IsNullOrEmpty(clearTextPassword))
                return new MainResult
                {
                    Success = false,
                    ErrorMessage = "Password cannot be null"
                };
            bool success = true;
            string error = "";
            var masterKeyResult = RetrieveHashedMasterKeyBase64();
            if (!masterKeyResult.MainResult.Success)
            {
                success = false;
                error = masterKeyResult.MainResult.ErrorMessage;
            }
            else
            {
                try
                {
                    bool isNew = false;
                    if (!File.Exists(Global.DataFileLocation))
                    {
                        File.WriteAllText(Global.DataFileLocation, null);
                        isNew = true;
                    }
                    var unlockDataFileResult = UnlockFile
                        (
                            filename:Global.DataFileLocation,
                            isNew:isNew
                         );
                    if (!unlockDataFileResult.Success)
                    {
                        success = false;
                        error = unlockDataFileResult.ErrorMessage;
                    }
                    else
                    {
                        byte[] encryptedDataBytes = null;
                        try
                        {
                            encryptedDataBytes = File.ReadAllBytes(Global.DataFileLocation);
                        }
                        catch { }
                        if (encryptedDataBytes == null)
                        {
                            success = false;
                            error = "Error while reading data";
                        }
                        else if(encryptedDataBytes.Length!=0)
                        {
                            byte[] decryptedDataBytes = new AESCrypto().Decrypt(encryptedDataBytes, masterKeyResult.Data);
                            if (decryptedDataBytes == null)
                            {
                                success = false;
                                error = "Error while decrypting data";
                            }
                            else
                            {
                                var changePasswordResult = StoreMasterKey(clearTextPassword);
                                if(!changePasswordResult.Success)
                                {
                                    success = false;
                                    error = changePasswordResult.ErrorMessage;
                                }
                                else
                                {

                                    masterKeyResult = RetrieveHashedMasterKeyBase64();
                                    if(!masterKeyResult.MainResult.Success)
                                    {
                                        success = false;
                                        error = masterKeyResult.MainResult.ErrorMessage;
                                    }
                                    else
                                    {
                                        decryptedDataBytes = new AESCrypto().Encrypt(decryptedDataBytes, masterKeyResult.Data);
                                        if (decryptedDataBytes == null)
                                        {
                                            success = false;
                                            error = "An error occured during data encryption";
                                        }
                                        else
                                        {
                                            bool save = true;
                                            try
                                            {
                                                File.WriteAllBytes(Global.DataFileLocation, decryptedDataBytes);
                                            }
                                            catch { save = false; }
                                            if (!save)
                                            {
                                                success = false;
                                                error = "An error occured while saving data";
                                            }
                                            var lockResult = LockFile(Global.DataFileLocation);
                                            if (!lockResult.Success)
                                            {
                                                success = false;
                                                error = lockResult.ErrorMessage;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var changePasswordResult = StoreMasterKey(clearTextPassword);
                            if (!changePasswordResult.Success)
                            {
                                success = false;
                                error = changePasswordResult.ErrorMessage;
                            }
                            else
                            {
                                var lockResult = LockFile(Global.DataFileLocation);
                                if (!lockResult.Success)
                                {
                                    success = false;
                                    error = lockResult.ErrorMessage;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    success = false;
                    error = ex.ToString();
                }
            }
            return new MainResult
            {
                Success = success,
                ErrorMessage = error
            };
        }
    }
}
