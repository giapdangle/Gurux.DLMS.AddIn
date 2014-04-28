//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/DlmsWizardPage1Dlg.cs $
//
// Version:         $Revision: 3947 $,
//                  $Date: 2011-08-31 14:27:28 +0300 (Wed, 31 Aug 2011) $
//                  $Author: kurumi $
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using Gurux.DLMS;
using Gurux.Device.Editor;
using Gurux.Device;
using Gurux.Common;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
	/// <summary>
	/// A DLMS specific custom wizard page. The page is used with the GXWizardDlg class.
	/// </summary>
	internal class DlmsWizardPage1Dlg : System.Windows.Forms.Form, IGXWizardPage
	{
        GXAuthentication SelectedAuthentication;
        private System.Windows.Forms.Label manufacturerTypeLbl;
		private System.Windows.Forms.ComboBox manufacturerTypeCb;
        private GXDevice m_Device;
		private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label authLevelLbl;
		private System.Windows.Forms.TextBox passwordTb;
		private System.Windows.Forms.Label passwordLbl;
        private System.Windows.Forms.ComboBox authLevelCb;
        private GroupBox AddressingGB;
        private ComboBox AddressTypeCB;
		private Label ServerIDTypeLbl;
		private NumericUpDown PhysicalAddressTB;
        private Label PhysicalAddressLbl;
        private NumericUpDown LogicalAddressTB;
        private Label LogicalAddressLbl;
        private Button UpdateOnlineBtn;
		private System.ComponentModel.Container m_Components = null;

		/// <summary>
		/// Initializes a new instance of the DlmsWizardPage1Dlg class.
		/// </summary>
        public DlmsWizardPage1Dlg(GXDevice device)
        {
            InitializeComponent();                        
            UpdateResources();
            AddressTypeCB.DrawMode = DrawMode.OwnerDrawFixed;
            this.TopLevel = false;
            this.FormBorderStyle = FormBorderStyle.None;
            m_Device = device;
            UpdateManufacturers();
            this.authLevelCb.SelectedIndexChanged += new System.EventHandler(this.AuthLevelCb_SelectedIndexChanged);
            UpdatemanufactureSettings(((GXDLMSDevice)m_Device).PhysicalAddress == null);
            this.manufacturerTypeCb.SelectedIndexChanged += new System.EventHandler(this.manufacturerTypeCb_SelectedIndexChanged);
            this.AddressTypeCB.SelectedIndexChanged += new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
        }

        void UpdateManufacturers()
        {
            manufacturerTypeCb.Items.Clear();
            ((GXDLMSDevice)m_Device).Manufacturers = new GXManufacturerCollection();
            GXManufacturerCollection.ReadManufacturerSettings(((GXDLMSDevice)m_Device).Manufacturers);
            foreach (GXManufacturer it in ((GXDLMSDevice)m_Device).Manufacturers)
            {
                manufacturerTypeCb.Items.Add(it);
            }

            //Manufacturer type
            foreach (GXManufacturer it in manufacturerTypeCb.Items)
            {
                if (it.Identification == ((GXDLMSDevice)m_Device).Identification)
                {
                    manufacturerTypeCb.SelectedItem = it;
                    break;
                }
            }

            //Select first manufacturer.
            if (manufacturerTypeCb.SelectedIndex == -1 && manufacturerTypeCb.Items.Count > 0)
            {
                manufacturerTypeCb.SelectedIndex = 0;
            } 
        }

		private void UpdateResources()
		{
			this.manufacturerTypeLbl.Text = Gurux.DLMS.AddIn.Properties.Resources.ManufacturerTypeTxt;
            this.passwordLbl.Text = Gurux.DLMS.AddIn.Properties.Resources.PasswordTxt;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (m_Components != null)
				{
					m_Components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
            this.manufacturerTypeCb = new System.Windows.Forms.ComboBox();
            this.manufacturerTypeLbl = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.passwordTb = new System.Windows.Forms.TextBox();
            this.passwordLbl = new System.Windows.Forms.Label();
            this.authLevelCb = new System.Windows.Forms.ComboBox();
            this.authLevelLbl = new System.Windows.Forms.Label();
            this.AddressingGB = new System.Windows.Forms.GroupBox();
            this.LogicalAddressTB = new System.Windows.Forms.NumericUpDown();
            this.LogicalAddressLbl = new System.Windows.Forms.Label();
            this.AddressTypeCB = new System.Windows.Forms.ComboBox();
            this.ServerIDTypeLbl = new System.Windows.Forms.Label();
            this.PhysicalAddressTB = new System.Windows.Forms.NumericUpDown();
            this.PhysicalAddressLbl = new System.Windows.Forms.Label();
            this.UpdateOnlineBtn = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.AddressingGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogicalAddressTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhysicalAddressTB)).BeginInit();
            this.SuspendLayout();
            // 
            // manufacturerTypeCb
            // 
            this.manufacturerTypeCb.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.manufacturerTypeCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.manufacturerTypeCb.Location = new System.Drawing.Point(116, 16);
            this.manufacturerTypeCb.Name = "manufacturerTypeCb";
            this.manufacturerTypeCb.Size = new System.Drawing.Size(188, 21);
            this.manufacturerTypeCb.TabIndex = 0;
            this.manufacturerTypeCb.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.manufacturerTypeCb_DrawItem);
            // 
            // manufacturerTypeLbl
            // 
            this.manufacturerTypeLbl.Location = new System.Drawing.Point(8, 16);
            this.manufacturerTypeLbl.Name = "manufacturerTypeLbl";
            this.manufacturerTypeLbl.Size = new System.Drawing.Size(100, 16);
            this.manufacturerTypeLbl.TabIndex = 0;
            this.manufacturerTypeLbl.Text = "Manufacturer Type";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.passwordTb);
            this.groupBox2.Controls.Add(this.passwordLbl);
            this.groupBox2.Controls.Add(this.authLevelCb);
            this.groupBox2.Controls.Add(this.authLevelLbl);
            this.groupBox2.Location = new System.Drawing.Point(11, 43);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(406, 79);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Authentication";
            // 
            // passwordTb
            // 
            this.passwordTb.Enabled = false;
            this.passwordTb.Location = new System.Drawing.Point(104, 48);
            this.passwordTb.Name = "passwordTb";
            this.passwordTb.PasswordChar = '*';
            this.passwordTb.Size = new System.Drawing.Size(286, 20);
            this.passwordTb.TabIndex = 3;
            // 
            // passwordLbl
            // 
            this.passwordLbl.Enabled = false;
            this.passwordLbl.Location = new System.Drawing.Point(8, 48);
            this.passwordLbl.Name = "passwordLbl";
            this.passwordLbl.Size = new System.Drawing.Size(80, 24);
            this.passwordLbl.TabIndex = 17;
            this.passwordLbl.Text = "Password";
            // 
            // authLevelCb
            // 
            this.authLevelCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authLevelCb.Location = new System.Drawing.Point(104, 24);
            this.authLevelCb.Name = "authLevelCb";
            this.authLevelCb.Size = new System.Drawing.Size(286, 21);
            this.authLevelCb.TabIndex = 2;
            // 
            // authLevelLbl
            // 
            this.authLevelLbl.Location = new System.Drawing.Point(8, 24);
            this.authLevelLbl.Name = "authLevelLbl";
            this.authLevelLbl.Size = new System.Drawing.Size(80, 16);
            this.authLevelLbl.TabIndex = 14;
            this.authLevelLbl.Text = "Level";
            // 
            // AddressingGB
            // 
            this.AddressingGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AddressingGB.Controls.Add(this.LogicalAddressTB);
            this.AddressingGB.Controls.Add(this.LogicalAddressLbl);
            this.AddressingGB.Controls.Add(this.AddressTypeCB);
            this.AddressingGB.Controls.Add(this.ServerIDTypeLbl);
            this.AddressingGB.Controls.Add(this.PhysicalAddressTB);
            this.AddressingGB.Controls.Add(this.PhysicalAddressLbl);
            this.AddressingGB.Location = new System.Drawing.Point(11, 117);
            this.AddressingGB.Name = "AddressingGB";
            this.AddressingGB.Size = new System.Drawing.Size(405, 84);
            this.AddressingGB.TabIndex = 32;
            this.AddressingGB.TabStop = false;
            this.AddressingGB.Text = "Addressing";
            // 
            // LogicalAddressTB
            // 
            this.LogicalAddressTB.Hexadecimal = true;
            this.LogicalAddressTB.Location = new System.Drawing.Point(305, 43);
            this.LogicalAddressTB.Maximum = new decimal(new int[] {
            0,
            1,
            0,
            0});
            this.LogicalAddressTB.Name = "LogicalAddressTB";
            this.LogicalAddressTB.Size = new System.Drawing.Size(85, 20);
            this.LogicalAddressTB.TabIndex = 6;
            this.LogicalAddressTB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LogicalAddressLbl
            // 
            this.LogicalAddressLbl.AutoSize = true;
            this.LogicalAddressLbl.Location = new System.Drawing.Point(217, 45);
            this.LogicalAddressLbl.Name = "LogicalAddressLbl";
            this.LogicalAddressLbl.Size = new System.Drawing.Size(85, 13);
            this.LogicalAddressLbl.TabIndex = 24;
            this.LogicalAddressLbl.Text = "Logical Address:";
            // 
            // AddressTypeCB
            // 
            this.AddressTypeCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddressTypeCB.FormattingEnabled = true;
            this.AddressTypeCB.ItemHeight = 13;
            this.AddressTypeCB.Location = new System.Drawing.Point(104, 16);
            this.AddressTypeCB.Name = "AddressTypeCB";
            this.AddressTypeCB.Size = new System.Drawing.Size(286, 21);
            this.AddressTypeCB.TabIndex = 4;
            this.AddressTypeCB.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ServerIDTypeCB_DrawItem);
            // 
            // ServerIDTypeLbl
            // 
            this.ServerIDTypeLbl.AutoSize = true;
            this.ServerIDTypeLbl.Location = new System.Drawing.Point(8, 19);
            this.ServerIDTypeLbl.Name = "ServerIDTypeLbl";
            this.ServerIDTypeLbl.Size = new System.Drawing.Size(34, 13);
            this.ServerIDTypeLbl.TabIndex = 21;
            this.ServerIDTypeLbl.Text = "Type:";
            // 
            // PhysicalAddressTB
            // 
            this.PhysicalAddressTB.Hexadecimal = true;
            this.PhysicalAddressTB.Location = new System.Drawing.Point(104, 43);
            this.PhysicalAddressTB.Maximum = new decimal(new int[] {
            0,
            1,
            0,
            0});
            this.PhysicalAddressTB.Name = "PhysicalAddressTB";
            this.PhysicalAddressTB.Size = new System.Drawing.Size(85, 20);
            this.PhysicalAddressTB.TabIndex = 5;
            this.PhysicalAddressTB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // PhysicalAddressLbl
            // 
            this.PhysicalAddressLbl.AutoSize = true;
            this.PhysicalAddressLbl.Location = new System.Drawing.Point(8, 45);
            this.PhysicalAddressLbl.Name = "PhysicalAddressLbl";
            this.PhysicalAddressLbl.Size = new System.Drawing.Size(90, 13);
            this.PhysicalAddressLbl.TabIndex = 8;
            this.PhysicalAddressLbl.Text = "Physical Address:";
            // 
            // UpdateOnlineBtn
            // 
            this.UpdateOnlineBtn.Location = new System.Drawing.Point(314, 14);
            this.UpdateOnlineBtn.Name = "UpdateOnlineBtn";
            this.UpdateOnlineBtn.Size = new System.Drawing.Size(87, 24);
            this.UpdateOnlineBtn.TabIndex = 1;
            this.UpdateOnlineBtn.Text = "Update online";
            this.UpdateOnlineBtn.Click += new System.EventHandler(this.UpdateOnlineBtn_Click);
            // 
            // DlmsWizardPage1Dlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(420, 228);
            this.Controls.Add(this.UpdateOnlineBtn);
            this.Controls.Add(this.AddressingGB);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.manufacturerTypeLbl);
            this.Controls.Add(this.manufacturerTypeCb);
            this.Name = "DlmsWizardPage1Dlg";
            this.Text = "WizardPagesDlg";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.AddressingGB.ResumeLayout(false);
            this.AddressingGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogicalAddressTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhysicalAddressTB)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		#region IGXWizardPage Members

		public void Back()
		{
		}

		public void Next()
		{
            
		}     

		/// <summary>
		/// Gets a string that describes the wizard page object.
		/// </summary>
		public string Description
		{
			get
			{
				return Gurux.DLMS.AddIn.Properties.Resources.WizardPage1DescriptionTxt;
			}
		}

		public string Caption
		{
			get
			{
                return Gurux.DLMS.AddIn.Properties.Resources.WizardPage1HeaderTxt;
			}
		}

		public GXWizardButtons EnabledButtons
		{
			get
			{
				return GXWizardButtons.All;
			}
		}

		public void Finish()
		{
            GXServerAddress target = (GXServerAddress)AddressTypeCB.SelectedItem;
            GXManufacturer man = (GXManufacturer)manufacturerTypeCb.SelectedItem;
            ((GXDLMSDevice)m_Device).ManufacturerName = man.Name;
            ((GXDLMSDevice)m_Device).Identification = man.Identification;
            ((GXDLMSDevice)m_Device).Authentication = (Gurux.DLMS.Authentication)authLevelCb.SelectedIndex;
            ((GXDLMSDevice)m_Device).UseLogicalNameReferencing = man.UseLogicalNameReferencing;
            ((GXDLMSDevice)m_Device).SupportNetworkSpecificSettings = man.UseIEC47;
            if (authLevelCb.SelectedIndex != 0)
            {
                ((GXDLMSDevice)m_Device).Password = passwordTb.Text;
            }
            GXAuthentication auth = man.GetAuthentication(Authentication.None);            
            if (auth != null)
            {
                ((GXDLMSDevice)m_Device).ClientID = auth.ClientID;
            }
            auth = man.GetAuthentication(Authentication.Low);
            if (auth != null)
            {
                ((GXDLMSDevice)m_Device).ClientIDLow = auth.ClientID;
            }
            auth = man.GetAuthentication(Authentication.High);            
            if (auth != null)
            {
                ((GXDLMSDevice)m_Device).ClientIDHigh = auth.ClientID;
            }
            ((GXDLMSDevice)m_Device).HDLCAddressing = target.HDLCAddress;
            if (target.HDLCAddress == HDLCAddressType.SerialNumber)
            {
                ((GXDLMSDevice)m_Device).SerialNumber = PhysicalAddressTB.Value.ToString();
                ((GXDLMSDevice)m_Device).SNFormula = man.GetServer(target.HDLCAddress).Formula;
                //We need physical address type, but we do not want to show it.
                ((GXDLMSDevice)m_Device).PhysicalAddress = Convert.ChangeType(this.PhysicalAddressTB.Value, target.PhysicalAddress.GetType());
            }
            else
            {
                ((GXDLMSDevice)m_Device).PhysicalAddress = Convert.ChangeType(this.PhysicalAddressTB.Value, target.PhysicalAddress.GetType());
                if (target.PhysicalAddress.GetType() == typeof(byte))
                {
                    ((GXDLMSDevice)m_Device).PhysicalAddressSize = 1;
                }
                else if (target.PhysicalAddress.GetType() == typeof(UInt16))
                {
                    ((GXDLMSDevice)m_Device).PhysicalAddressSize = 2;
                }
                if (target.PhysicalAddress.GetType() == typeof(UInt32))
                {
                    ((GXDLMSDevice)m_Device).PhysicalAddressSize = 4;
                }
            }
            
            ((GXDLMSDevice)m_Device).LogicalAddress = Convert.ToInt32(this.LogicalAddressTB.Value);
		}		

		public void Initialize()
		{
		}

		public void Cancel()
		{
		}
	
		public bool IsShown()
		{
			return true;
		}

        object IGXWizardPage.Target
        {
            get;
            set;
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

        private void manufacturerTypeCb_DrawItem(object sender, DrawItemEventArgs e)
        {
            // If the index is invalid then simply exit.
            if (e.Index == -1 || e.Index >= manufacturerTypeCb.Items.Count)
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
            GXManufacturer target = (GXManufacturer)manufacturerTypeCb.Items[e.Index];
            if (target == null)
            {
                return;
            }
            string name = target.Name;
            SizeF s = e.Graphics.MeasureString(name, f);
            e.Graphics.DrawString(name, f, b, e.Bounds);
        }

        private void AuthLevelCb_SelectedIndexChanged(object sender, System.EventArgs e)
        {            
            SelectedAuthentication = (GXAuthentication)authLevelCb.SelectedItem;
            ((GXDLMSDevice)m_Device).Authentication = (Gurux.DLMS.Authentication)SelectedAuthentication.Type;
            bool enabled = ((GXDLMSDevice)m_Device).Authentication != (int) Gurux.DLMS.Authentication.None;
            if (enabled)
            {
                passwordTb.Text = ((GXDLMSDevice)m_Device).Password;
            }
            passwordTb.Enabled = enabled;
        }

        void UpdatemanufactureSettings(bool initialize)
        {
            GXManufacturer man = (GXManufacturer)manufacturerTypeCb.SelectedItem;
			if (man == null)
			{
				return;
			}
            if (initialize)
            {
                ((GXDLMSDevice)m_Device).StartProtocol = man.StartProtocol;
            }
            authLevelCb.Items.Clear();
            AddressTypeCB.Items.Clear();
          	AddressTypeCB.Items.AddRange(man.ServerSettings.ToArray());
            if (initialize)
            {
                this.AddressTypeCB.SelectedIndexChanged += new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
                AddressTypeCB.SelectedItem = man.GetActiveServer();
                this.AddressTypeCB.SelectedIndexChanged -= new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
            }
            else
            {
                AddressTypeCB.SelectedItem = man.GetServer(((GXDLMSDevice)m_Device).HDLCAddressing);
                if (AddressTypeCB.SelectedItem == null)
                {
                    this.AddressTypeCB.SelectedIndexChanged += new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
                    AddressTypeCB.SelectedItem = man.GetActiveServer();
                    this.AddressTypeCB.SelectedIndexChanged -= new System.EventHandler(this.AddressTypeCB_SelectedIndexChanged);
                }
                else
                {
                    this.PhysicalAddressTB.Value = Convert.ToDecimal(((GXDLMSDevice)m_Device).PhysicalAddress);
                    this.LogicalAddressTB.Value = ((GXDLMSDevice)m_Device).LogicalAddress;
                }
            }
            //This must call because there are obx lists where are no authentications.
            man.GetActiveAuthentication();
            foreach (GXAuthentication it in man.Settings)
            {
                int pos = authLevelCb.Items.Add(it);
                if (initialize)
                {
                    if (it.Selected)
                    {                        
                        authLevelCb.SelectedIndex = pos;                        
                    }
                }
                else if (((GXDLMSDevice)m_Device).Authentication == it.Type)
                {
                    authLevelCb.SelectedIndex = pos;
                }
            }
            if (authLevelCb.Items.Count != 0 && authLevelCb.SelectedIndex == -1)
            {                
                authLevelCb.SelectedIndex = 0;                
            }
        }

        private void manufacturerTypeCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatemanufactureSettings(true);
        }

        private void UpdateOnlineBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string backupPath = GXManufacturerCollection.UpdateManufactureSettings();
                if (string.IsNullOrEmpty(backupPath))
                {
                    System.Windows.Forms.MessageBox.Show(this, "Manufacturer settings updated.", "GXDeviceEditor");
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(this, "Manufacturer settings updated. Old settings are saved to: " + backupPath, "GXDeviceEditor");
                }
                UpdateManufacturers();
            }
            catch (Exception Ex)
            {
                GXCommon.ShowError(this, Ex);
            }
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
            this.LogicalAddressTB.Value = target.LogicalAddress;
        }
	}
}
