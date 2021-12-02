using System;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TaskTrayApplication
{
    public class TaskTrayApplicationContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();
        Configuration configWindow = new Configuration();
        private static string localIP = string.Empty;

        public TaskTrayApplicationContext()
        {
            //MenuItem configMenuItem = new MenuItem("Configuration", new EventHandler(ShowConfig));
            MenuItem staticIP = new MenuItem("固定 IP", new EventHandler(ChangeStatic));
            MenuItem floatingIP = new MenuItem("浮動 IP", new EventHandler(ChangeFloating));
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(Exit));
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;

            notifyIcon.Icon = Properties.Resources.None;
            notifyIcon.DoubleClick += new EventHandler(ShowMessage);
            //notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { configMenuItem, staticIP, floatingIP, exitMenuItem });
            notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { staticIP, floatingIP, exitMenuItem });
            notifyIcon.Visible = true;
            GetCurrentIP();
        }

        void ShowMessage(object sender, EventArgs e)
        {
            // Only show the message if the settings say we can.
            if (TaskTrayApplication.Properties.Settings.Default.ShowMessage)
                MessageBox.Show("Hello World");
        }

        void ShowConfig(object sender, EventArgs e)
        {
            // If we are already showing the window meerly focus it.
            if (configWindow.Visible)
                configWindow.Focus();
            else
                configWindow.ShowDialog();
        }

        void ChangeStatic(object sender, EventArgs e)
        {
            Enable("固定IP");
            Disable("DHCP");
        }

        void ChangeFloating(object sender, EventArgs e)
        {
            Enable("DHCP");
            Disable("固定IP");
        }

        private static void Enable(string interfaceName)
        {
            System.Diagnostics.ProcessStartInfo psi =
                   new System.Diagnostics.ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" enable");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.Start();
        }
        private static void Disable(string interfaceName)
        {
            System.Diagnostics.ProcessStartInfo psi =
                new System.Diagnostics.ProcessStartInfo("netsh", "interface set interface \"" + interfaceName + "\" disable");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = psi;
            psi.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.Start();
        }

        void Exit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.
            notifyIcon.Visible = false;

            Application.Exit();
        }

        void AddressChangedCallback(object sender, EventArgs e)
        {
            GetCurrentIP();
        }

        private void GetCurrentIP()
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                try
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (localIP != endPoint.Address.ToString())
                    {
                        localIP = endPoint.Address.ToString();
                    }
                }
                catch (SocketException)
                {
                    localIP = string.Empty;
                }
                finally
                {
                    if (localIP.StartsWith("163"))
                    {
                        notifyIcon.Icon = Properties.Resources.Static;
                    }
                    else if (localIP.StartsWith("192"))
                    {
                        notifyIcon.Icon = Properties.Resources.DHCP;
                    }
                    else
                    {
                        notifyIcon.Icon = Properties.Resources.None;
                    }
                }

            }
        }
    }
}
