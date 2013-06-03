using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gurux.Common;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.Net;

namespace Gurux.DLMS.AddIn
{
    public partial class GXDLMSDeviceUI : Form, IGXPropertyPage
    {
        GXDLMSDevice m_Device;

        public GXDLMSDeviceUI(GXDLMSDevice device)
        {
            m_Device = device;            
            InitializeComponent();
            AddressTypeCB.DrawMode = DrawMode.OwnerDrawFixed;
            this.authLevelCb.SelectedIndexChanged += new System.EventHandler(this.AuthLevelCb_SelectedIndexChanged);
            this.AddressTypeCB.SelectedIndexChanged += new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
            authLevelCb.Items.Add(Authentication.None);
            authLevelCb.Items.Add(Authentication.Low);
            authLevelCb.Items.Add(Authentication.High);
            authLevelCb.SelectedItem = m_Device.Authentication;
            StartProtocolCB.Items.Add(StartProtocolType.IEC);
            StartProtocolCB.Items.Add(StartProtocolType.DLMS);
            StartProtocolCB.SelectedItem = m_Device.StartProtocol;
            GXClient_OnMediaStateChange(null, null);
        }

        /// <summary>
        /// Use Remote serial is only available for network media.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GXClient_OnMediaStateChange(object sender, MediaStateEventArgs e)
        {
            UseRemoteSerialCB.Enabled = m_Device.GXClient.Media is GXNet;            
        }

        #region IGXPropertyPage Members

        public void Initialize()
        {
            m_Device.GXClient.OnMediaStateChange += new MediaStateChangeEventHandler(GXClient_OnMediaStateChange);
        }

        public void Apply()
        {
            m_Device.GXClient.OnMediaStateChange -= new MediaStateChangeEventHandler(GXClient_OnMediaStateChange);
        }

        #endregion

        /// <summary>
        /// Draw device ID types.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerIDTypeCB_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= AddressTypeCB.Items.Count)
            {
                return;
            }

            // Draw the background of the item.
            e.DrawBackground();

            // Should we draw the focus rectangle?
            if ((e.State & DrawItemState.Focus) != 0)
            {
                e.DrawFocusRectangle();
            }

            Font f = new Font(e.Font, FontStyle.Regular);
            // Create a new background brush.
            Brush b = new SolidBrush(e.ForeColor);
            // Draw the item.			
            GXServerAddress target = (GXServerAddress)AddressTypeCB.Items[e.Index];
            if (target == null)
            {
                return;
            }
            string name = target.HDLCAddress.ToString();
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }
        
        private void AuthLevelCb_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Authentication auth = (Authentication)authLevelCb.SelectedItem;
            bool enabled = auth != Authentication.None;
            if (enabled)
            {
                passwordTb.Text = ((GXDLMSDevice)m_Device).Password;
            }
            passwordTb.Enabled = enabled;
        }

        private void AddressTypeCB_SelectedIndexChanged(object sender, EventArgs e)
        {
            GXServerAddress target = (GXServerAddress)AddressTypeCB.SelectedItem;
            bool sn = target.HDLCAddress == HDLCAddressType.SerialNumber;
            PhysicalAddressTB.Hexadecimal = !sn;
            if (sn)
            {
                PhysicalAddressLbl.Text = "Serial number:";
            }
            else
            {
                PhysicalAddressLbl.Text = "Physical Address:";
            }
            this.PhysicalAddressTB.Value = Convert.ToDecimal(target.PhysicalAddress);
            this.LocicalAddressTB.Value = target.LogicalAddress;
        }        
    }
}
