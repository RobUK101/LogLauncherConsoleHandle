using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace LogLauncherConsoleHandler
{
    class Program
    {
        static RegistryHive returnregistryHive(string registryClass)
        {
            if (registryClass == "HKEY_LOCAL_MACHINE")
            {
                return RegistryHive.LocalMachine;
            }


            if (registryClass == "HKEY_CURRENT_USER")
            {
                return RegistryHive.CurrentUser;
            }

            return RegistryHive.LocalMachine;
        }

        static object getregkeyValue(string remoteServer, string regClass, string regPath, string regKey)
        {
            try
            {
                RegistryKey hk = RegistryKey.OpenRemoteBaseKey(returnregistryHive(regClass), remoteServer);

                if (hk != null)
                {
                    hk = hk.OpenSubKey(regPath);

                    if (hk != null)
                    {
                        Object regResult = hk.GetValue(regKey);

                        if (regResult != null)
                        {
                            return regResult;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        static bool regkeyvalueExist(string remoteServer, string regClass, string regKey, string regValue)
        {
            try
            {
                RegistryKey remoteHK = RegistryKey.OpenRemoteBaseKey(returnregistryHive(regClass), remoteServer);

                remoteHK = remoteHK.OpenSubKey(regKey);

                Object regResult = remoteHK.GetValue(regValue);

                remoteHK.Close();

                if (regResult == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        static string getmsiinstallPath(string msiProduct)
        {
            try
            {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

                foreach (var regValue in regKey.GetSubKeyNames())
                {
                    RegistryKey msiproductKey = regKey.OpenSubKey(regValue);
                    
                    foreach (var value in msiproductKey.GetValueNames())
                    {
                        if (value == "DisplayName")
                        {
                            string productName = Convert.ToString(msiproductKey.GetValue("DisplayName"));

                            if (productName == msiProduct)

                            {
                                return Convert.ToString(msiproductKey.GetValue("InstallLocation") + @"\LogLauncher.exe");
                            }

                        }
                    }
                }

                return null;
            }
            catch (Exception ee)
            {
                return null;
            }        
        }

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                string argumentSupplied = args[0];

                if (argumentSupplied == "INSTALL")
                {
                    try
                    {
                        if (regkeyvalueExist("", "HKEY_LOCAL_MACHINE", @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup", "UI Installation Directory"))
                        {
                            string cmconsolePath = getregkeyValue("", "HKEY_LOCAL_MACHINE", @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup", "UI Installation Directory").ToString();

                            string loglauncherPath = getmsiinstallPath("LogLauncher");

                            if ((!File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39\LogLauncher.xml")) || (!File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62\LogLauncher.xml")))
                            {
                                // Create Action folders if they do not already exist

                                string[] logLauncherXML = new[] { "<ActionDescription Class=" + (char)34 + "Executable" + (char)34 + " DisplayName=" + (char)34 + "Log Launcher" + (char)34 + " MnemonicDisplayName=" + (char)34 + "Log Launcher" + (char)34 + " Description=" + (char)34 + "Launchers Log Launcher" + (char)34 + " RibbonDisplayType=" + (char)34 + "TextAndSmallImage" + (char)34 + "><ShowOn><string>ContextMenu</string><string>DefaultHomeTab</string></ShowOn><Executable><FilePath>" + (char)34 + loglauncherPath + (char)34 + "</FilePath><Parameters> " + (char)34 + "##SUB:Name##" + (char)34 + "</Parameters></Executable></ActionDescription>" };

                                if (!Directory.Exists(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39"))
                                {
                                    Directory.CreateDirectory(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39");
                                }

                                if (!Directory.Exists(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62"))
                                {
                                    Directory.CreateDirectory(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62");
                                }

                                // Create the XML files if they do not already exist

                                if (!File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39\LogLauncher.xml"))
                                {
                                    System.IO.File.WriteAllLines(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39\LogLauncher.xml", logLauncherXML);
                                }

                                if (!File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62\LogLauncher.xml"))
                                {
                                    System.IO.File.WriteAllLines(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62\LogLauncher.xml", logLauncherXML);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                }

                if (argumentSupplied == "UNINSTALL")
                {
                    try
                    {
                        if (regkeyvalueExist("", "HKEY_LOCAL_MACHINE", @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup", "UI Installation Directory")) // Check if the Console is installed
                        {
                            string cmconsolePath = getregkeyValue("", "HKEY_LOCAL_MACHINE", @"SOFTWARE\Wow6432Node\Microsoft\ConfigMgr10\Setup", "UI Installation Directory").ToString(); // Locate Console path

                            if ((File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39\LogLauncher.xml")) || (!File.Exists(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62\LogLauncher.xml"))) // Check if extensions are in place
                            {
                                System.IO.File.Delete(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39\LogLauncher.xml"); // Delete first extension

                                System.IO.File.Delete(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62\LogLauncher.xml"); // Delete Second extension
                            }

                            if (Directory.GetFiles(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62", "*", SearchOption.AllDirectories).Length == 0) // Check if extension folders are empty, delete if so
                            {
                                System.IO.Directory.Delete(cmconsolePath + @"xmlstorage\extensions\actions\ed9dee86-eadd-4ac8-82a1-7234a4646e62");
                            }

                            if (Directory.GetFiles(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39", "*", SearchOption.AllDirectories).Length == 0)
                            {
                                System.IO.Directory.Delete(cmconsolePath + @"xmlstorage\extensions\actions\3fd01cd1-9e01-461e-92cd-94866b8d1f39");
                            }

                        }
                    }
                    catch (Exception ee)
                    {

                    }
                }
            }
        }
    }
}
