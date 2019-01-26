using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using Microsoft.Win32;

namespace TestTask
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var companyName = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Test company");
                var productName = companyName.CreateSubKey("Test product");
                productName.SetValue("URL", "localhost", RegistryValueKind.String);
                Logger.Log("Key created");


                var users = new List<string>();
                var query = new SelectQuery("Win32_UserAccount");
                var searcher = new ManagementObjectSearcher(query);
                var skippedUser = "headstone";
                foreach (ManagementObject envVar in searcher.Get())
                {
                    var username = (string) envVar["Name"];
                    if (username == skippedUser) continue;
                    users.Add($@"{Environment.MachineName}\\{username}");
                }

                var rs = new RegistrySecurity();
                foreach (var user in users)
                {
                    rs.AddAccessRule(new RegistryAccessRule(user, RegistryRights.ReadKey, AccessControlType.Deny));
                }

                productName.SetAccessControl(rs);
                Logger.Log("Permissions set");
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                throw;
            }
        }

        protected override void OnStop()
        {
        }
    }
}