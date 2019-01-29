using System;
using System.Collections.Generic;
using System.Management;
using System.Security.AccessControl;
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
                var productName = CreateRegistryKey();
                SetPermissions(productName);
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                throw;
            }
        }

        private static void SetPermissions(RegistryKey productName)
        {
            // Get all users except "skippedUser" 
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

            // Remove read permissions from all users in list
            var rs = new RegistrySecurity();
            foreach (var user in users)
            {
                rs.AddAccessRule(new RegistryAccessRule(user, RegistryRights.ReadKey, AccessControlType.Deny));
            }

            productName.SetAccessControl(rs);
            Logger.Log("Permissions set");
        }

        private RegistryKey CreateRegistryKey()
        {
            var companyName = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Test company");
            var productName = companyName.CreateSubKey("Test product");
            productName.SetValue("URL", "localhost", RegistryValueKind.String);
            Logger.Log("Key created");
            return productName;
        }

        protected override void OnStop()
        {
        }
    }
}