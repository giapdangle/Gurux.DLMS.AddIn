//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXObisEditor/ObisMainForm.cs $
//
// Version:         $Revision: 870 $,
//                  $Date: 2009-09-29 17:21:48 +0300 (ti, 29 syys 2009) $
//                  $Author: airija $
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
using System.Data;

namespace GXObisEditor
{
	/// <summary>
	/// Summary description for ObisMainForm.
	/// </summary>
	public class ObisMainForm : System.Windows.Forms.Form
	{
		#region definitions
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.TreeView ObisTree;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem FileMnu;
		private System.Windows.Forms.MenuItem OpenMnu;
		private System.Windows.Forms.MenuItem SaveMnu;
		private System.Windows.Forms.MenuItem SplitterMnu;
		private System.Windows.Forms.MenuItem ExitMnu;
		private System.Windows.Forms.Label NameLbl;
		private System.Windows.Forms.TextBox NameTb;
		private System.Windows.Forms.TextBox TypeTb;
		private System.Windows.Forms.Label TypeLbl;
		private System.Windows.Forms.TextBox DescriptionTb;
		private System.Windows.Forms.Label DescriptionLbl;
		private System.Windows.Forms.ComboBox InterfaceCb;
		private System.Windows.Forms.TextBox InterfaceTb;
		private System.Windows.Forms.Button RemoveInterfaceBtn;
		private System.Windows.Forms.Button AddInterfaceBtn;
		private System.Windows.Forms.Label InterfacesLbl;
		private System.Windows.Forms.Button ApplyBtn;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.MenuItem NewMnu;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton OpenTBtn;
		private System.Windows.Forms.ToolBarButton SaveTBtn;
		private System.Windows.Forms.ToolBarButton Split1TBtn;
		private System.Windows.Forms.ToolBarButton AddTBtn;
		private System.Windows.Forms.ToolBarButton RemoveTBtn;
		private System.Windows.Forms.MenuItem ToolsMnu;
		private System.Windows.Forms.MenuItem AddMnu;
		private System.Windows.Forms.MenuItem RemoveMnu;
		private System.Windows.Forms.ToolBarButton NewTBtn;
		private System.Windows.Forms.ImageList ToolBarImageList;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem AddContextMnu;
		private System.Windows.Forms.MenuItem RemoveContextMnu;
		private System.Windows.Forms.Button ResetBtn;
		private System.Windows.Forms.MenuItem SaveAsMnu;
		private ObisManufacturer m_Manufacturer = null;
		bool m_ClearSaveFailed = false;
		private System.Windows.Forms.TextBox PrimaryIDTb;
		private System.Windows.Forms.Label PrimaryIDLbl;
		private System.Windows.Forms.Label PrimaryIDSizeLbl;
		private System.Windows.Forms.TextBox PrimaryIDSizeTb;
		private System.Windows.Forms.TextBox SecondaryIDSizeTb;
		private System.Windows.Forms.Label SecondaryIDSizeLbl;
		private System.Windows.Forms.TextBox SecondaryIDTb;
		private System.Windows.Forms.Label SecondaryIDLbl;
		private System.Windows.Forms.CheckBox UseLNCb;
		bool m_Dirty = false;
		System.Resources.ResourceManager m_Resources = null;
		private System.Windows.Forms.MenuItem TestMnu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem LanguageMnu;
		GuruxDLMS.CCDLMSOBISParser m_parser = null;
		private System.Windows.Forms.TextBox LowAccessIDSizeTb;
		private System.Windows.Forms.Label LowAccessIDSizeLbl;
		private System.Windows.Forms.TextBox LowAccessIDTb;
		private System.Windows.Forms.Label LowAccessIDLbl;
		private System.Windows.Forms.TextBox HighAccessIDSizeTb;
		private System.Windows.Forms.Label HighAccessIDSizeLbl;
		private System.Windows.Forms.TextBox HighAccessIDTb;
		private System.Windows.Forms.Label HighAccessIDLbl;
		private System.Windows.Forms.CheckBox SupportNetworkSpecificSettingsCb;
		string m_CurrentLanguage = string.Empty;
		#endregion

		public ObisMainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_Resources = new System.Resources.ResourceManager("GXObisEditor.strings", this.GetType().Assembly);
			UpdateResources();

			Application.Idle += new EventHandler(Application_Idle);
			m_parser = new GuruxDLMS.CCDLMSOBISParser();
			
			SetupResources();
		}

		private void SetupResources()
		{
			string lang = null;
			Microsoft.Win32.RegistryKey subKey = null;
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Gurux\\GXDeviceEditor");
			if(key != null)
			{
				subKey = key.OpenSubKey("General");
			}
			lang = "en";
			if(subKey != null)
			{
				lang = (string) subKey.GetValue("Language");
			}
			try
			{
				System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(lang);
			}
			catch//Set english as default language.
			{
				lang = "en";
				System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(lang);
			}

			MenuItem item;
			foreach(System.Globalization.CultureInfo it in GetAvailableLanguages())
			{
				string str = it.NativeName;
				int len = str.IndexOf("(");
				if (len > 0)
				{
					str = str.Substring(0, len);
				}
				item = this.LanguageMnu.MenuItems.Add(str);
				item.Click += new System.EventHandler(this.OnChangeLanguage);

				if (lang == it.TwoLetterISOLanguageName)
				{
					item.Checked = true;
				}
			}

			UpdateResources();
		}

		/// <summary>
		/// Update resources
		/// </summary>
		void UpdateResources()
		{
			this.Text = m_Resources.GetString("ApplicationTxt");
			FileMnu.Text = m_Resources.GetString("FileTxt");
			NewTBtn.Text = NewMnu.Text = m_Resources.GetString("NewTxt");
			OpenTBtn.Text = OpenMnu.Text = m_Resources.GetString("OpenTxt");
			SaveTBtn.Text = SaveMnu.Text = m_Resources.GetString("SaveTxt");
			SaveAsMnu.Text = m_Resources.GetString("SaveAsTxt");
			ExitMnu.Text = m_Resources.GetString("ExitTxt");
			ToolsMnu.Text = m_Resources.GetString("ToolsTxt");
			AddTBtn.Text = AddMnu.Text = m_Resources.GetString("AddTxt");
			RemoveTBtn.Text = RemoveMnu.Text = m_Resources.GetString("RemoveTxt");
			NameLbl.Text = m_Resources.GetString("NameTxt");
			TypeLbl.Text = m_Resources.GetString("TypeTxt");
			DescriptionLbl.Text = m_Resources.GetString("DecriptionTxt");
			InterfacesLbl.Text = m_Resources.GetString("InterfacesTxt");
			AddInterfaceBtn.Text = m_Resources.GetString("AddInterfaceTxt");
			RemoveInterfaceBtn.Text = m_Resources.GetString("RemoveInterfaceTxt");
			//Bad translations and couldn't come up with better ones
			//PrimaryIDLbl.Text = m_Resources.GetString("PrimaryIDTxt");
			//PrimaryIDSizeLbl.Text = m_Resources.GetString("PrimaryIDSizeTxt");
			//SecondaryIDLbl.Text = m_Resources.GetString("SecondaryIDTxt");
			//SecondaryIDSizeLbl.Text = m_Resources.GetString("SecondaryIDSizeTxt");
			UseLNCb.Text = m_Resources.GetString("UseLNCbTxt");
			SupportNetworkSpecificSettingsCb.Text = m_Resources.GetString("SupportNetworkSpecificSettingsTxt");
			ApplyBtn.Text = m_Resources.GetString("ApplyTxt");
			ResetBtn.Text = m_Resources.GetString("ResetTxt");
			TestMnu.Text = m_Resources.GetString("TestObisCodesTxt");
			LanguageMnu.Text = m_Resources.GetString("LanguageTxt");
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ObisMainForm));
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.SupportNetworkSpecificSettingsCb = new System.Windows.Forms.CheckBox();
			this.HighAccessIDSizeTb = new System.Windows.Forms.TextBox();
			this.HighAccessIDSizeLbl = new System.Windows.Forms.Label();
			this.HighAccessIDTb = new System.Windows.Forms.TextBox();
			this.HighAccessIDLbl = new System.Windows.Forms.Label();
			this.LowAccessIDSizeTb = new System.Windows.Forms.TextBox();
			this.LowAccessIDSizeLbl = new System.Windows.Forms.Label();
			this.LowAccessIDTb = new System.Windows.Forms.TextBox();
			this.LowAccessIDLbl = new System.Windows.Forms.Label();
			this.UseLNCb = new System.Windows.Forms.CheckBox();
			this.SecondaryIDSizeTb = new System.Windows.Forms.TextBox();
			this.SecondaryIDSizeLbl = new System.Windows.Forms.Label();
			this.SecondaryIDTb = new System.Windows.Forms.TextBox();
			this.SecondaryIDLbl = new System.Windows.Forms.Label();
			this.PrimaryIDSizeTb = new System.Windows.Forms.TextBox();
			this.PrimaryIDSizeLbl = new System.Windows.Forms.Label();
			this.PrimaryIDTb = new System.Windows.Forms.TextBox();
			this.PrimaryIDLbl = new System.Windows.Forms.Label();
			this.ResetBtn = new System.Windows.Forms.Button();
			this.NameTb = new System.Windows.Forms.TextBox();
			this.TypeTb = new System.Windows.Forms.TextBox();
			this.TypeLbl = new System.Windows.Forms.Label();
			this.InterfaceCb = new System.Windows.Forms.ComboBox();
			this.RemoveInterfaceBtn = new System.Windows.Forms.Button();
			this.NameLbl = new System.Windows.Forms.Label();
			this.InterfacesLbl = new System.Windows.Forms.Label();
			this.ApplyBtn = new System.Windows.Forms.Button();
			this.DescriptionTb = new System.Windows.Forms.TextBox();
			this.DescriptionLbl = new System.Windows.Forms.Label();
			this.InterfaceTb = new System.Windows.Forms.TextBox();
			this.AddInterfaceBtn = new System.Windows.Forms.Button();
			this.panel2 = new System.Windows.Forms.Panel();
			this.ObisTree = new System.Windows.Forms.TreeView();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.AddContextMnu = new System.Windows.Forms.MenuItem();
			this.RemoveContextMnu = new System.Windows.Forms.MenuItem();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.FileMnu = new System.Windows.Forms.MenuItem();
			this.NewMnu = new System.Windows.Forms.MenuItem();
			this.OpenMnu = new System.Windows.Forms.MenuItem();
			this.SaveMnu = new System.Windows.Forms.MenuItem();
			this.SaveAsMnu = new System.Windows.Forms.MenuItem();
			this.SplitterMnu = new System.Windows.Forms.MenuItem();
			this.ExitMnu = new System.Windows.Forms.MenuItem();
			this.ToolsMnu = new System.Windows.Forms.MenuItem();
			this.AddMnu = new System.Windows.Forms.MenuItem();
			this.RemoveMnu = new System.Windows.Forms.MenuItem();
			this.TestMnu = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.LanguageMnu = new System.Windows.Forms.MenuItem();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.NewTBtn = new System.Windows.Forms.ToolBarButton();
			this.OpenTBtn = new System.Windows.Forms.ToolBarButton();
			this.SaveTBtn = new System.Windows.Forms.ToolBarButton();
			this.Split1TBtn = new System.Windows.Forms.ToolBarButton();
			this.AddTBtn = new System.Windows.Forms.ToolBarButton();
			this.RemoveTBtn = new System.Windows.Forms.ToolBarButton();
			this.ToolBarImageList = new System.Windows.Forms.ImageList(this.components);
			this.panel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(226, 42);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(406, 399);
			this.panel1.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.SupportNetworkSpecificSettingsCb);
			this.groupBox1.Controls.Add(this.HighAccessIDSizeTb);
			this.groupBox1.Controls.Add(this.HighAccessIDSizeLbl);
			this.groupBox1.Controls.Add(this.HighAccessIDTb);
			this.groupBox1.Controls.Add(this.HighAccessIDLbl);
			this.groupBox1.Controls.Add(this.LowAccessIDSizeTb);
			this.groupBox1.Controls.Add(this.LowAccessIDSizeLbl);
			this.groupBox1.Controls.Add(this.LowAccessIDTb);
			this.groupBox1.Controls.Add(this.LowAccessIDLbl);
			this.groupBox1.Controls.Add(this.UseLNCb);
			this.groupBox1.Controls.Add(this.SecondaryIDSizeTb);
			this.groupBox1.Controls.Add(this.SecondaryIDSizeLbl);
			this.groupBox1.Controls.Add(this.SecondaryIDTb);
			this.groupBox1.Controls.Add(this.SecondaryIDLbl);
			this.groupBox1.Controls.Add(this.PrimaryIDSizeTb);
			this.groupBox1.Controls.Add(this.PrimaryIDSizeLbl);
			this.groupBox1.Controls.Add(this.PrimaryIDTb);
			this.groupBox1.Controls.Add(this.PrimaryIDLbl);
			this.groupBox1.Controls.Add(this.ResetBtn);
			this.groupBox1.Controls.Add(this.NameTb);
			this.groupBox1.Controls.Add(this.TypeTb);
			this.groupBox1.Controls.Add(this.TypeLbl);
			this.groupBox1.Controls.Add(this.InterfaceCb);
			this.groupBox1.Controls.Add(this.RemoveInterfaceBtn);
			this.groupBox1.Controls.Add(this.NameLbl);
			this.groupBox1.Controls.Add(this.InterfacesLbl);
			this.groupBox1.Controls.Add(this.ApplyBtn);
			this.groupBox1.Controls.Add(this.DescriptionTb);
			this.groupBox1.Controls.Add(this.DescriptionLbl);
			this.groupBox1.Controls.Add(this.InterfaceTb);
			this.groupBox1.Controls.Add(this.AddInterfaceBtn);
			this.groupBox1.Location = new System.Drawing.Point(4, 4);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(396, 346);
			this.groupBox1.TabIndex = 12;
			this.groupBox1.TabStop = false;
			// 
			// SupportNetworkSpecificSettingsCb
			// 
			this.SupportNetworkSpecificSettingsCb.Location = new System.Drawing.Point(120, 292);
			this.SupportNetworkSpecificSettingsCb.Name = "SupportNetworkSpecificSettingsCb";
			this.SupportNetworkSpecificSettingsCb.Size = new System.Drawing.Size(256, 16);
			this.SupportNetworkSpecificSettingsCb.TabIndex = 30;
			this.SupportNetworkSpecificSettingsCb.Text = "Support Network Specific Settings";
			// 
			// HighAccessIDSizeTb
			// 
			this.HighAccessIDSizeTb.Enabled = false;
			this.HighAccessIDSizeTb.Location = new System.Drawing.Point(292, 216);
			this.HighAccessIDSizeTb.Name = "HighAccessIDSizeTb";
			this.HighAccessIDSizeTb.Size = new System.Drawing.Size(88, 20);
			this.HighAccessIDSizeTb.TabIndex = 29;
			this.HighAccessIDSizeTb.Text = "";
			// 
			// HighAccessIDSizeLbl
			// 
			this.HighAccessIDSizeLbl.Location = new System.Drawing.Point(260, 216);
			this.HighAccessIDSizeLbl.Name = "HighAccessIDSizeLbl";
			this.HighAccessIDSizeLbl.Size = new System.Drawing.Size(32, 16);
			this.HighAccessIDSizeLbl.TabIndex = 28;
			this.HighAccessIDSizeLbl.Text = "Size:";
			// 
			// HighAccessIDTb
			// 
			this.HighAccessIDTb.Enabled = false;
			this.HighAccessIDTb.Location = new System.Drawing.Point(120, 216);
			this.HighAccessIDTb.Name = "HighAccessIDTb";
			this.HighAccessIDTb.Size = new System.Drawing.Size(136, 20);
			this.HighAccessIDTb.TabIndex = 27;
			this.HighAccessIDTb.Text = "";
			// 
			// HighAccessIDLbl
			// 
			this.HighAccessIDLbl.Location = new System.Drawing.Point(8, 216);
			this.HighAccessIDLbl.Name = "HighAccessIDLbl";
			this.HighAccessIDLbl.Size = new System.Drawing.Size(108, 16);
			this.HighAccessIDLbl.TabIndex = 26;
			this.HighAccessIDLbl.Text = "High Access ID:";
			// 
			// LowAccessIDSizeTb
			// 
			this.LowAccessIDSizeTb.Enabled = false;
			this.LowAccessIDSizeTb.Location = new System.Drawing.Point(292, 192);
			this.LowAccessIDSizeTb.Name = "LowAccessIDSizeTb";
			this.LowAccessIDSizeTb.Size = new System.Drawing.Size(88, 20);
			this.LowAccessIDSizeTb.TabIndex = 25;
			this.LowAccessIDSizeTb.Text = "";
			// 
			// LowAccessIDSizeLbl
			// 
			this.LowAccessIDSizeLbl.Location = new System.Drawing.Point(260, 192);
			this.LowAccessIDSizeLbl.Name = "LowAccessIDSizeLbl";
			this.LowAccessIDSizeLbl.Size = new System.Drawing.Size(32, 16);
			this.LowAccessIDSizeLbl.TabIndex = 24;
			this.LowAccessIDSizeLbl.Text = "Size:";
			// 
			// LowAccessIDTb
			// 
			this.LowAccessIDTb.Enabled = false;
			this.LowAccessIDTb.Location = new System.Drawing.Point(120, 192);
			this.LowAccessIDTb.Name = "LowAccessIDTb";
			this.LowAccessIDTb.Size = new System.Drawing.Size(136, 20);
			this.LowAccessIDTb.TabIndex = 23;
			this.LowAccessIDTb.Text = "";
			// 
			// LowAccessIDLbl
			// 
			this.LowAccessIDLbl.Location = new System.Drawing.Point(8, 192);
			this.LowAccessIDLbl.Name = "LowAccessIDLbl";
			this.LowAccessIDLbl.Size = new System.Drawing.Size(108, 16);
			this.LowAccessIDLbl.TabIndex = 22;
			this.LowAccessIDLbl.Text = "Low Access ID:";
			// 
			// UseLNCb
			// 
			this.UseLNCb.Location = new System.Drawing.Point(120, 272);
			this.UseLNCb.Name = "UseLNCb";
			this.UseLNCb.Size = new System.Drawing.Size(256, 16);
			this.UseLNCb.TabIndex = 21;
			this.UseLNCb.Text = "Use LN";
			// 
			// SecondaryIDSizeTb
			// 
			this.SecondaryIDSizeTb.Enabled = false;
			this.SecondaryIDSizeTb.Location = new System.Drawing.Point(292, 244);
			this.SecondaryIDSizeTb.Name = "SecondaryIDSizeTb";
			this.SecondaryIDSizeTb.Size = new System.Drawing.Size(88, 20);
			this.SecondaryIDSizeTb.TabIndex = 20;
			this.SecondaryIDSizeTb.Text = "";
			// 
			// SecondaryIDSizeLbl
			// 
			this.SecondaryIDSizeLbl.Location = new System.Drawing.Point(260, 244);
			this.SecondaryIDSizeLbl.Name = "SecondaryIDSizeLbl";
			this.SecondaryIDSizeLbl.Size = new System.Drawing.Size(32, 16);
			this.SecondaryIDSizeLbl.TabIndex = 19;
			this.SecondaryIDSizeLbl.Text = "Size:";
			// 
			// SecondaryIDTb
			// 
			this.SecondaryIDTb.Enabled = false;
			this.SecondaryIDTb.Location = new System.Drawing.Point(120, 244);
			this.SecondaryIDTb.Name = "SecondaryIDTb";
			this.SecondaryIDTb.Size = new System.Drawing.Size(136, 20);
			this.SecondaryIDTb.TabIndex = 18;
			this.SecondaryIDTb.Text = "";
			// 
			// SecondaryIDLbl
			// 
			this.SecondaryIDLbl.Location = new System.Drawing.Point(8, 244);
			this.SecondaryIDLbl.Name = "SecondaryIDLbl";
			this.SecondaryIDLbl.Size = new System.Drawing.Size(108, 16);
			this.SecondaryIDLbl.TabIndex = 17;
			this.SecondaryIDLbl.Text = "Server ID:";
			// 
			// PrimaryIDSizeTb
			// 
			this.PrimaryIDSizeTb.Enabled = false;
			this.PrimaryIDSizeTb.Location = new System.Drawing.Point(292, 168);
			this.PrimaryIDSizeTb.Name = "PrimaryIDSizeTb";
			this.PrimaryIDSizeTb.Size = new System.Drawing.Size(88, 20);
			this.PrimaryIDSizeTb.TabIndex = 16;
			this.PrimaryIDSizeTb.Text = "";
			// 
			// PrimaryIDSizeLbl
			// 
			this.PrimaryIDSizeLbl.Location = new System.Drawing.Point(260, 168);
			this.PrimaryIDSizeLbl.Name = "PrimaryIDSizeLbl";
			this.PrimaryIDSizeLbl.Size = new System.Drawing.Size(32, 16);
			this.PrimaryIDSizeLbl.TabIndex = 15;
			this.PrimaryIDSizeLbl.Text = "Size:";
			// 
			// PrimaryIDTb
			// 
			this.PrimaryIDTb.Enabled = false;
			this.PrimaryIDTb.Location = new System.Drawing.Point(120, 168);
			this.PrimaryIDTb.Name = "PrimaryIDTb";
			this.PrimaryIDTb.Size = new System.Drawing.Size(136, 20);
			this.PrimaryIDTb.TabIndex = 14;
			this.PrimaryIDTb.Text = "";
			// 
			// PrimaryIDLbl
			// 
			this.PrimaryIDLbl.Location = new System.Drawing.Point(8, 168);
			this.PrimaryIDLbl.Name = "PrimaryIDLbl";
			this.PrimaryIDLbl.Size = new System.Drawing.Size(108, 16);
			this.PrimaryIDLbl.TabIndex = 13;
			this.PrimaryIDLbl.Text = "Default Client ID:";
			// 
			// ResetBtn
			// 
			this.ResetBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResetBtn.Enabled = false;
			this.ResetBtn.Location = new System.Drawing.Point(324, 314);
			this.ResetBtn.Name = "ResetBtn";
			this.ResetBtn.Size = new System.Drawing.Size(64, 24);
			this.ResetBtn.TabIndex = 12;
			this.ResetBtn.Text = "Reset";
			this.ResetBtn.Click += new System.EventHandler(this.ResetBtn_Click);
			// 
			// NameTb
			// 
			this.NameTb.Enabled = false;
			this.NameTb.Location = new System.Drawing.Point(120, 20);
			this.NameTb.Name = "NameTb";
			this.NameTb.Size = new System.Drawing.Size(260, 20);
			this.NameTb.TabIndex = 1;
			this.NameTb.Text = "";
			// 
			// TypeTb
			// 
			this.TypeTb.Enabled = false;
			this.TypeTb.Location = new System.Drawing.Point(120, 44);
			this.TypeTb.Name = "TypeTb";
			this.TypeTb.Size = new System.Drawing.Size(260, 20);
			this.TypeTb.TabIndex = 3;
			this.TypeTb.Text = "";
			// 
			// TypeLbl
			// 
			this.TypeLbl.Location = new System.Drawing.Point(8, 44);
			this.TypeLbl.Name = "TypeLbl";
			this.TypeLbl.Size = new System.Drawing.Size(108, 16);
			this.TypeLbl.TabIndex = 2;
			this.TypeLbl.Text = "Type:";
			// 
			// InterfaceCb
			// 
			this.InterfaceCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.InterfaceCb.Enabled = false;
			this.InterfaceCb.Location = new System.Drawing.Point(120, 140);
			this.InterfaceCb.Name = "InterfaceCb";
			this.InterfaceCb.Size = new System.Drawing.Size(196, 21);
			this.InterfaceCb.TabIndex = 9;
			// 
			// RemoveInterfaceBtn
			// 
			this.RemoveInterfaceBtn.Enabled = false;
			this.RemoveInterfaceBtn.Location = new System.Drawing.Point(320, 140);
			this.RemoveInterfaceBtn.Name = "RemoveInterfaceBtn";
			this.RemoveInterfaceBtn.Size = new System.Drawing.Size(60, 20);
			this.RemoveInterfaceBtn.TabIndex = 10;
			this.RemoveInterfaceBtn.Text = "Remove";
			this.RemoveInterfaceBtn.Click += new System.EventHandler(this.RemoveInterfaceBtn_Click);
			// 
			// NameLbl
			// 
			this.NameLbl.Location = new System.Drawing.Point(8, 20);
			this.NameLbl.Name = "NameLbl";
			this.NameLbl.Size = new System.Drawing.Size(108, 16);
			this.NameLbl.TabIndex = 0;
			this.NameLbl.Text = "Name:";
			// 
			// InterfacesLbl
			// 
			this.InterfacesLbl.Location = new System.Drawing.Point(8, 116);
			this.InterfacesLbl.Name = "InterfacesLbl";
			this.InterfacesLbl.Size = new System.Drawing.Size(108, 16);
			this.InterfacesLbl.TabIndex = 6;
			this.InterfacesLbl.Text = "Interfaces:";
			// 
			// ApplyBtn
			// 
			this.ApplyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplyBtn.Enabled = false;
			this.ApplyBtn.Location = new System.Drawing.Point(252, 314);
			this.ApplyBtn.Name = "ApplyBtn";
			this.ApplyBtn.Size = new System.Drawing.Size(64, 24);
			this.ApplyBtn.TabIndex = 11;
			this.ApplyBtn.Text = "Apply";
			this.ApplyBtn.Click += new System.EventHandler(this.ApplyBtn_Click);
			// 
			// DescriptionTb
			// 
			this.DescriptionTb.Enabled = false;
			this.DescriptionTb.Location = new System.Drawing.Point(120, 68);
			this.DescriptionTb.Multiline = true;
			this.DescriptionTb.Name = "DescriptionTb";
			this.DescriptionTb.Size = new System.Drawing.Size(260, 40);
			this.DescriptionTb.TabIndex = 5;
			this.DescriptionTb.Text = "";
			// 
			// DescriptionLbl
			// 
			this.DescriptionLbl.Location = new System.Drawing.Point(8, 68);
			this.DescriptionLbl.Name = "DescriptionLbl";
			this.DescriptionLbl.Size = new System.Drawing.Size(108, 16);
			this.DescriptionLbl.TabIndex = 4;
			this.DescriptionLbl.Text = "Decription:";
			// 
			// InterfaceTb
			// 
			this.InterfaceTb.Enabled = false;
			this.InterfaceTb.Location = new System.Drawing.Point(120, 116);
			this.InterfaceTb.Name = "InterfaceTb";
			this.InterfaceTb.Size = new System.Drawing.Size(196, 20);
			this.InterfaceTb.TabIndex = 7;
			this.InterfaceTb.Text = "";
			// 
			// AddInterfaceBtn
			// 
			this.AddInterfaceBtn.Enabled = false;
			this.AddInterfaceBtn.Location = new System.Drawing.Point(320, 116);
			this.AddInterfaceBtn.Name = "AddInterfaceBtn";
			this.AddInterfaceBtn.Size = new System.Drawing.Size(60, 20);
			this.AddInterfaceBtn.TabIndex = 8;
			this.AddInterfaceBtn.Text = "Add";
			this.AddInterfaceBtn.Click += new System.EventHandler(this.AddInterfaceBtn_Click);
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.ObisTree);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel2.Location = new System.Drawing.Point(0, 42);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(224, 399);
			this.panel2.TabIndex = 1;
			// 
			// ObisTree
			// 
			this.ObisTree.ContextMenu = this.contextMenu1;
			this.ObisTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ObisTree.HideSelection = false;
			this.ObisTree.ImageIndex = -1;
			this.ObisTree.Location = new System.Drawing.Point(0, 0);
			this.ObisTree.Name = "ObisTree";
			this.ObisTree.SelectedImageIndex = -1;
			this.ObisTree.Size = new System.Drawing.Size(224, 399);
			this.ObisTree.TabIndex = 0;
			this.ObisTree.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ObisTree_MouseDown);
			this.ObisTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ObisTree_AfterSelect);
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.AddContextMnu,
																						 this.RemoveContextMnu});
			// 
			// AddContextMnu
			// 
			this.AddContextMnu.Index = 0;
			this.AddContextMnu.Text = "Add";
			this.AddContextMnu.Click += new System.EventHandler(this.AddMnu_Click);
			// 
			// RemoveContextMnu
			// 
			this.RemoveContextMnu.Index = 1;
			this.RemoveContextMnu.Text = "Remove";
			this.RemoveContextMnu.Click += new System.EventHandler(this.RemoveMnu_Click);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(224, 42);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(2, 399);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.FileMnu,
																					  this.ToolsMnu});
			// 
			// FileMnu
			// 
			this.FileMnu.Index = 0;
			this.FileMnu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.NewMnu,
																					this.OpenMnu,
																					this.SaveMnu,
																					this.SaveAsMnu,
																					this.SplitterMnu,
																					this.ExitMnu});
			this.FileMnu.Text = "&File";
			// 
			// NewMnu
			// 
			this.NewMnu.Index = 0;
			this.NewMnu.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
			this.NewMnu.Text = "&New";
			this.NewMnu.Click += new System.EventHandler(this.NewMnu_Click);
			// 
			// OpenMnu
			// 
			this.OpenMnu.Index = 1;
			this.OpenMnu.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.OpenMnu.Text = "&Open";
			this.OpenMnu.Click += new System.EventHandler(this.OpenMnu_Click);
			// 
			// SaveMnu
			// 
			this.SaveMnu.Enabled = false;
			this.SaveMnu.Index = 2;
			this.SaveMnu.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.SaveMnu.Text = "&Save";
			this.SaveMnu.Click += new System.EventHandler(this.SaveMnu_Click);
			// 
			// SaveAsMnu
			// 
			this.SaveAsMnu.Enabled = false;
			this.SaveAsMnu.Index = 3;
			this.SaveAsMnu.Text = "Save as...";
			this.SaveAsMnu.Click += new System.EventHandler(this.SaveAsMnu_Click);
			// 
			// SplitterMnu
			// 
			this.SplitterMnu.Index = 4;
			this.SplitterMnu.Text = "-";
			// 
			// ExitMnu
			// 
			this.ExitMnu.Index = 5;
			this.ExitMnu.Text = "E&xit";
			this.ExitMnu.Click += new System.EventHandler(this.ExitMnu_Click);
			// 
			// ToolsMnu
			// 
			this.ToolsMnu.Index = 1;
			this.ToolsMnu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.AddMnu,
																					 this.RemoveMnu,
																					 this.TestMnu,
																					 this.menuItem1,
																					 this.LanguageMnu});
			this.ToolsMnu.Text = "&Tools";
			// 
			// AddMnu
			// 
			this.AddMnu.Enabled = false;
			this.AddMnu.Index = 0;
			this.AddMnu.Text = "&Add";
			this.AddMnu.Click += new System.EventHandler(this.AddMnu_Click);
			// 
			// RemoveMnu
			// 
			this.RemoveMnu.Enabled = false;
			this.RemoveMnu.Index = 1;
			this.RemoveMnu.Shortcut = System.Windows.Forms.Shortcut.Del;
			this.RemoveMnu.Text = "&Remove";
			this.RemoveMnu.Click += new System.EventHandler(this.RemoveMnu_Click);
			// 
			// TestMnu
			// 
			this.TestMnu.Enabled = false;
			this.TestMnu.Index = 2;
			this.TestMnu.Text = "Test";
			this.TestMnu.Click += new System.EventHandler(this.TestMnu_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 3;
			this.menuItem1.Text = "-";
			// 
			// LanguageMnu
			// 
			this.LanguageMnu.Index = 4;
			this.LanguageMnu.Text = "Language";
			// 
			// toolBar1
			// 
			this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.NewTBtn,
																						this.OpenTBtn,
																						this.SaveTBtn,
																						this.Split1TBtn,
																						this.AddTBtn,
																						this.RemoveTBtn});
			this.toolBar1.ButtonSize = new System.Drawing.Size(23, 23);
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.ImageList = this.ToolBarImageList;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(632, 42);
			this.toolBar1.TabIndex = 3;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// NewTBtn
			// 
			this.NewTBtn.ImageIndex = 0;
			this.NewTBtn.Text = "New";
			this.NewTBtn.ToolTipText = "New";
			// 
			// OpenTBtn
			// 
			this.OpenTBtn.ImageIndex = 1;
			this.OpenTBtn.Text = "Open";
			this.OpenTBtn.ToolTipText = "Open";
			// 
			// SaveTBtn
			// 
			this.SaveTBtn.Enabled = false;
			this.SaveTBtn.ImageIndex = 2;
			this.SaveTBtn.Text = "Save";
			this.SaveTBtn.ToolTipText = "Save";
			// 
			// Split1TBtn
			// 
			this.Split1TBtn.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// AddTBtn
			// 
			this.AddTBtn.Enabled = false;
			this.AddTBtn.ImageIndex = 3;
			this.AddTBtn.Text = "Add";
			this.AddTBtn.ToolTipText = "Add";
			// 
			// RemoveTBtn
			// 
			this.RemoveTBtn.Enabled = false;
			this.RemoveTBtn.ImageIndex = 9;
			this.RemoveTBtn.Text = "Remove";
			this.RemoveTBtn.ToolTipText = "Remove";
			// 
			// ToolBarImageList
			// 
			this.ToolBarImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.ToolBarImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ToolBarImageList.ImageStream")));
			this.ToolBarImageList.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// ObisMainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 441);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.toolBar1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "ObisMainForm";
			this.Text = "GXObisEditor";
			this.panel1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ObisMainForm());
		}

		private void BuildTree()
		{
			ObisTree.Nodes.Clear();
			TreeNode rootNode = new TreeNode(m_Manufacturer.Name);
			rootNode.Tag = m_Manufacturer;
			ObisTree.Nodes.Add(rootNode);

			TreeNode commonNode = new TreeNode("Common Properties");
			commonNode.Tag = m_Manufacturer.CommonProperties;
			foreach (ObisItem it in m_Manufacturer.CommonProperties)
			{
				TreeNode itemNode = new TreeNode(it.LN);
				itemNode.Tag = it;
				commonNode.Nodes.Add(itemNode);
			}
			rootNode.Nodes.Add(commonNode);

			foreach (ObisDevice dev in m_Manufacturer.Devices)
			{
				TreeNode devNode = new TreeNode(dev.Name);
				devNode.Tag = dev;
				foreach (ObisItem it in dev.Items)
				{
					TreeNode itemNode = new TreeNode(it.LN);
					itemNode.Tag = it;
					devNode.Nodes.Add(itemNode);
				}
				rootNode.Nodes.Add(devNode);
			}
			ObisTree.SelectedNode = rootNode;
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			try
			{
				if (e.Button == NewTBtn)
				{
					NewMnu_Click(null, null);
				}
				else if (e.Button == OpenTBtn)
				{
					OpenMnu_Click(null, null);
				}
				else if (e.Button == SaveTBtn)
				{
					SaveMnu_Click(null, null);
				}
				else if (e.Button == AddTBtn)
				{
					AddMnu_Click(null, null);
				}
				else if (e.Button == RemoveTBtn)
				{
					RemoveMnu_Click(null, null);
				}
			}
			catch (Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}
		}

		private void NewMnu_Click(object sender, System.EventArgs e)
		{
			if (!BeforeClearCheck())
			{
				return;
			}
			m_Manufacturer = new ObisManufacturer("Manufacturer", 0, "");
			TestMnu.Enabled = SaveAsMnu.Enabled = SaveMnu.Enabled = SaveTBtn.Enabled = true;
			BuildTree();
		}

		private void OpenMnu_Click(object sender, System.EventArgs e)
		{
			if (!BeforeClearCheck())
			{
				return;
			}
			ObisManufacturer mnf = new ObisManufacturer();
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.ValidateNames = true;
			dlg.Filter = "XML OBIS files (*.obx)|*.obx|All files (*.*)|*.*";
			dlg.Multiselect = false;
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				mnf.Load(dlg.FileName);
				m_Manufacturer = mnf;
				BuildTree();
				TestMnu.Enabled = SaveAsMnu.Enabled = SaveMnu.Enabled = SaveTBtn.Enabled = true;
			}
			m_Dirty = false;
		}

		private void SaveMnu_Click(object sender, System.EventArgs e)
		{
			if (m_Manufacturer.FilePath == string.Empty)
			{
				SaveAsMnu_Click(null, null);
			}
			else
			{
				m_Manufacturer.Save(m_Manufacturer.FilePath);
			}
			m_Dirty = false;
		}

		private void ExitMnu_Click(object sender, System.EventArgs e)
		{
			if (BeforeClearCheck())
			{
				this.Close();
			}
		}

		private void AddMnu_Click(object sender, System.EventArgs e)
		{
			if (ObisTree.SelectedNode == null || ObisTree.SelectedNode.Tag == null)
			{
				//Do nothing, because this should not happen
			}
			else if (ObisTree.SelectedNode.Tag is ObisManufacturer)
			{
				//Add device
				ObisDevice dev = new ObisDevice("Device");
				m_Manufacturer.Devices.Add(dev);
				TreeNode newNode = new TreeNode("Device");
				newNode.Tag = dev;
				ObisTree.SelectedNode.Nodes.Add(newNode);
				ObisTree.SelectedNode.Expand();
				ObisTree.SelectedNode = newNode;
			}
			else if(ObisTree.SelectedNode.Tag is ObisDevice)
			{
				ObisItem newItem = AddItem(((ObisDevice) ObisTree.SelectedNode.Tag).Items);
				TreeNode newNode = new TreeNode(newItem.LN);
				newNode.Tag = newItem;
				ObisTree.SelectedNode.Nodes.Add(newNode);
				ObisTree.SelectedNode.Expand();
				ObisTree.SelectedNode = newNode;
			}
			else if(ObisTree.SelectedNode.Tag is ObisItems)
			{
				ObisItem newItem = AddItem((ObisItems)ObisTree.SelectedNode.Tag);
				TreeNode newNode = new TreeNode(newItem.LN);
				newNode.Tag = newItem;
				ObisTree.SelectedNode.Nodes.Add(newNode);
				ObisTree.SelectedNode.Expand();
				ObisTree.SelectedNode = newNode;
			}
			else if(ObisTree.SelectedNode.Tag is ObisItem)
			{
				ObisItem newItem = null;
				if (ObisTree.SelectedNode.Parent.Tag is ObisDevice)
				{
					newItem = AddItem(((ObisDevice) ObisTree.SelectedNode.Parent.Tag).Items);
				}
				else
				{
					newItem = AddItem(((ObisItems) ObisTree.SelectedNode.Parent.Tag));
				}
				TreeNode newNode = new TreeNode(newItem.LN);
				newNode.Tag = newItem;
				ObisTree.SelectedNode.Parent.Nodes.Add(newNode);
				ObisTree.SelectedNode = newNode;
			}
			m_Dirty = true;
		}

		private ObisItem AddItem(ObisItems parent)
		{
			ObisItem item = new ObisItem("1.2.3", 0, "");
			parent.Add(item);
			return item;
		}

		private void RemoveMnu_Click(object sender, System.EventArgs e)
		{
			if (!ObisTree.Focused)
			{
				return;
			}
			try
			{
				if(ObisTree.SelectedNode.Tag is ObisDevice)
				{
					ObisDevice dev = ((ObisDevice)ObisTree.SelectedNode.Tag);
					TreeNode removedNode = ObisTree.SelectedNode;
					ObisTree.SelectedNode = removedNode.Parent;
					ObisTree.SelectedNode.Nodes.Remove(removedNode);

					m_Manufacturer.Devices.Remove(dev);
					m_Dirty = true;
				}
				else if(ObisTree.SelectedNode.Tag is ObisItem)
				{
					ObisItem item = ((ObisItem)ObisTree.SelectedNode.Tag);
					if (ObisTree.SelectedNode.Parent.Tag is ObisItems)
					{
						m_Manufacturer.CommonProperties.Remove(item);
					}
					else //Under device
					{
						ObisDevice dev = (ObisDevice)ObisTree.SelectedNode.Parent.Tag;
						dev.Items.Remove(item);
					}
					TreeNode removedNode = ObisTree.SelectedNode;
					ObisTree.SelectedNode = removedNode.Parent;
					ObisTree.SelectedNode.Nodes.Remove(removedNode);
					m_Dirty = true;
				}
			}
			catch (Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}
		}

		private void SaveAsMnu_Click(object sender, System.EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.CheckPathExists = true;
			dlg.ValidateNames = true;
			dlg.Filter = "XML OBIS files (*.obx)|*.obx|All files (*.*)|*.*";

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				m_Manufacturer.Save(dlg.FileName);
				m_Dirty = false;
			}
			else
			{
				m_ClearSaveFailed = true;
			}
		}

		private bool BeforeClearCheck()
		{
			if (m_Manufacturer != null && m_Dirty)
			{
				DialogResult dr = MessageBox.Show(this, "Do you want to save current job?", "GXObisEditor", MessageBoxButtons.YesNoCancel);
				if (dr == DialogResult.Cancel)
				{
					return false;
				}
				else if (dr == DialogResult.Yes)
				{
					m_ClearSaveFailed = false;
					SaveMnu_Click(null, null);
					return !m_ClearSaveFailed;
				}
				else if (dr == DialogResult.No)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}
		}

		private void ObisTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			//Reset everything just in case
			AddContextMnu.Enabled = AddMnu.Enabled = AddTBtn.Enabled = RemoveContextMnu.Enabled = RemoveTBtn.Enabled = RemoveMnu.Enabled = false;
			InterfaceCb.Items.Clear();
			InterfaceTb.Text = string.Empty;
			NameLbl.Text = "Name:";
			NameTb.Text = string.Empty;
			DescriptionTb.Text = string.Empty;
			TypeTb.Text = string.Empty;
			foreach (Control ctrl in groupBox1.Controls)
			{
				if (ctrl.Name.IndexOf("ID") != -1 && ctrl is TextBox)
				{
					((TextBox) ctrl).Text = "";
				}
			}
			foreach (Control ctrl in groupBox1.Controls)
			{
				ctrl.Enabled = false;
			}

			if (ObisTree.SelectedNode != null && ObisTree.SelectedNode.Tag != null)
			{
				ApplyBtn.Enabled = ResetBtn.Enabled = true;
			}
			
			if (ObisTree.SelectedNode.Tag is ObisManufacturer)
			{
				AddContextMnu.Enabled = AddMnu.Enabled = AddTBtn.Enabled = true;
				NameTb.Enabled = NameLbl.Enabled = DescriptionTb.Enabled = DescriptionLbl.Enabled = TypeTb.Enabled = TypeLbl.Enabled = true;

				NameTb.Text = m_Manufacturer.Name;
				DescriptionTb.Text = m_Manufacturer.Description;
				TypeTb.Text = m_Manufacturer.Type.ToString();
			}
			else if(ObisTree.SelectedNode.Tag is ObisDevice)
			{
				AddContextMnu.Enabled = AddMnu.Enabled = AddTBtn.Enabled = RemoveContextMnu.Enabled = RemoveTBtn.Enabled = RemoveMnu.Enabled = true;
				SupportNetworkSpecificSettingsCb.Enabled = UseLNCb.Enabled = NameTb.Enabled = NameLbl.Enabled = DescriptionTb.Enabled = DescriptionLbl.Enabled = TypeTb.Enabled = TypeLbl.Enabled = true;
				foreach (Control ctrl in groupBox1.Controls)
				{
					if (ctrl.Name.IndexOf("ID") != -1)
					{
						ctrl.Enabled = true;
					}
				}

				ObisDevice dev = (ObisDevice) ObisTree.SelectedNode.Tag;
				NameTb.Text = dev.Name;
				DescriptionTb.Text = dev.Description;
				TypeTb.Text = dev.Type.ToString();
				PrimaryIDTb.Text = dev.PrimaryDefaultID.ToString();
				PrimaryIDSizeTb.Text = dev.PrimaryDefaultIDSize.ToString();
				LowAccessIDTb.Text = dev.PrimaryLowID.ToString();
				LowAccessIDSizeTb.Text = dev.PrimaryLowIDSize.ToString();
				HighAccessIDTb.Text = dev.PrimaryHighID.ToString();
				HighAccessIDSizeTb.Text = dev.PrimaryHighIDSize.ToString();
				SecondaryIDTb.Text = dev.SecondaryID.ToString();
				SecondaryIDSizeTb.Text = dev.SecondaryIDSize.ToString();
				UseLNCb.Checked = dev.UseLN;
				SupportNetworkSpecificSettingsCb.Checked = dev.SupportNetworkSpecificSettings;
			}
			else if(ObisTree.SelectedNode.Tag is ObisItems)
			{
				AddContextMnu.Enabled = AddMnu.Enabled = AddTBtn.Enabled = true;
				RemoveContextMnu.Enabled = RemoveTBtn.Enabled = RemoveMnu.Enabled = false;
			}
			else if(ObisTree.SelectedNode.Tag is ObisItem)
			{
				NameTb.Enabled = NameLbl.Enabled = DescriptionTb.Enabled = DescriptionLbl.Enabled = TypeTb.Enabled = TypeLbl.Enabled = true;
				foreach (Control ctrl in groupBox1.Controls)
				{
					if (ctrl.Name.ToLower().IndexOf("interface") != -1)
					{
						ctrl.Enabled = true;
					}
				}
				AddContextMnu.Enabled = AddMnu.Enabled = AddTBtn.Enabled = RemoveContextMnu.Enabled = RemoveTBtn.Enabled = RemoveMnu.Enabled = true;

				ObisItem item = (ObisItem) ObisTree.SelectedNode.Tag;
				NameLbl.Text = "LN:";
				NameTb.Text = item.LN;
				DescriptionTb.Text = item.Description;
				TypeTb.Text = item.Type.ToString();

				foreach (int obisInterface in item.Interfaces)
				{
					InterfaceCb.Items.Add(obisInterface.ToString());
				}
				if (item.Interfaces.Count != 0)
				{
					InterfaceCb.SelectedIndex = 0;
				}
			}
		}

		private void ObisTree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				ObisTree.SelectedNode = ObisTree.GetNodeAt(e.X, e.Y);
			}
		}

		private void ApplyBtn_Click(object sender, System.EventArgs e)
		{
			try
			{
				if (ObisTree.SelectedNode == null || ObisTree.SelectedNode.Tag == null)
				{
					throw new Exception("Nothing is selected in the tree.");
				}
				else if (NameTb.Text.Trim().Length == 0)
				{
					throw new Exception("Name can not be empty");
				}
				else if (ObisTree.SelectedNode.Tag is ObisManufacturer)
				{
					m_Manufacturer.Name = NameTb.Text;
					ObisTree.SelectedNode.Text = NameTb.Text;
					m_Manufacturer.Description = DescriptionTb.Text;
					m_Manufacturer.Type = ConvertToInt(TypeTb.Text);
				}
				else if(ObisTree.SelectedNode.Tag is ObisDevice)
				{
					ObisDevice dev = (ObisDevice) ObisTree.SelectedNode.Tag;

					dev.Name = NameTb.Text;
					ObisTree.SelectedNode.Text = NameTb.Text;
					dev.Description = DescriptionTb.Text;
					dev.Type = ConvertToInt(TypeTb.Text);
					dev.PrimaryDefaultID = ConvertToInt(PrimaryIDTb.Text);
					dev.PrimaryDefaultIDSize = ConvertToInt(PrimaryIDSizeTb.Text);
					dev.PrimaryLowID = ConvertToInt(LowAccessIDTb.Text);
					dev.PrimaryLowIDSize = ConvertToInt(LowAccessIDSizeTb.Text);
					dev.PrimaryHighID = ConvertToInt(HighAccessIDTb.Text);
					dev.PrimaryHighIDSize = ConvertToInt(HighAccessIDSizeTb.Text);
					dev.SecondaryID = ConvertToInt(SecondaryIDTb.Text);
					dev.SecondaryIDSize = ConvertToInt(SecondaryIDSizeTb.Text);
					dev.UseLN = UseLNCb.Checked;
					dev.SupportNetworkSpecificSettings = SupportNetworkSpecificSettingsCb.Checked;
				}
				else if(ObisTree.SelectedNode.Tag is ObisItem)
				{
					ObisItem item = (ObisItem) ObisTree.SelectedNode.Tag;

					item.LN = NameTb.Text;
					ObisTree.SelectedNode.Text = NameTb.Text;
					item.Description = DescriptionTb.Text;

					item.Type = ConvertToInt(TypeTb.Text);

					item.Interfaces.Clear();
					foreach (string iface in InterfaceCb.Items)
					{
						item.Interfaces.Add(ConvertToInt(iface));
					}
				}
				m_Dirty = true;
			}
			catch (Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}
		}

		private int ConvertToInt(string Input)
		{
			double dOut = 0;
			if (!double.TryParse(Input, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture, out dOut))
			{
				throw new Exception("Value must be an integer.");
			}
			return (int)dOut;
		}

		private void ResetBtn_Click(object sender, System.EventArgs e)
		{
			TreeNode selNode = ObisTree.SelectedNode;
			ObisTree.SelectedNode = null;
			ObisTree.SelectedNode = selNode;
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			string caption = string.Empty;
			if (m_Manufacturer == null)
			{
				caption = "GXObisEditor";
			}
			else
			{
				caption ="GXObisEditor - " + m_Manufacturer.Name;
				if (m_Dirty)
				{
					caption += "*";
				}
			}
			if (this.Text != caption)
			{
				this.Text = caption;
			}
		}

		private void AddInterfaceBtn_Click(object sender, System.EventArgs e)
		{
			try
			{
				ConvertToInt(InterfaceTb.Text);
				InterfaceCb.Items.Add(InterfaceTb.Text);
				InterfaceCb.SelectedIndex = InterfaceCb.Items.Count - 1;
			}
			catch (Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}
		}

		private void RemoveInterfaceBtn_Click(object sender, System.EventArgs e)
		{
			if (InterfaceCb.SelectedItem is string && InterfaceCb.SelectedItem.ToString().Length > 0)
			{
				InterfaceCb.Items.Remove(InterfaceCb.SelectedItem);
				if (InterfaceCb.Items.Count > 0)
				{
					InterfaceCb.SelectedIndex = 0;
				}
			}
		}

		private void HandleOBISID(System.Windows.Forms.TreeNodeCollection Nodes, bool Add)
		{
			foreach(TreeNode it in Nodes)
			{
				//Add sub items
				if (!(it.Tag is ObisItem))
				{
					HandleOBISID(it.Nodes, Add);
					return;
				}
				ObisItem item = (ObisItem) it.Tag;				
				if (Add)
				{
					m_parser.AddOBISCode(it.Text, (GuruxDLMS.DLMS_DATA_TYPE) item.Type, item.Description, item.Interfaces.Array);
				}
				int type = 0;
				if (item.Interfaces.Count != 0)
				{
					type = item.Interfaces[0];
				}
				if (!Add)
				{
					string desc = m_parser.GetDescription(it.Text, (GuruxDLMS.DLMS_OBJECT_TYPE) type);
					if (item.Description != desc)
					{
						MessageBox.Show("OBIS descriptions are different.\r\nGet: " + desc + "\r\nShould be: " + item.Description + "\r\n" + it.Text);
					}
				}
			}
		}


		/// <summary>
		/// Test all OBIS codes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TestMnu_Click(object sender, System.EventArgs e)
		{
			try
			{
				m_parser.ResetOBISCodes();
				//Add all OBIS Codes
				HandleOBISID(ObisTree.Nodes, true);
				//Find OBIS codes.
				HandleOBISID(ObisTree.Nodes, false);
				MessageBox.Show("Done");
			}
			catch(Exception Ex)
			{
				MessageBox.Show(Ex.Message);
			}
		}

		public System.Globalization.CultureInfo[] GetAvailableLanguages()
		{
			ArrayList list = new ArrayList();
			System.Globalization.CultureInfo info = System.Globalization.CultureInfo.CreateSpecificCulture("En");
			list.Add(info);
			// Get the current application path
			string strPath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(strPath);
			foreach(System.IO.DirectoryInfo it in di.GetDirectories())
			{
				try
				{
					info = System.Globalization.CultureInfo.CreateSpecificCulture(it.Name);
					list.Add(info);
				}
				catch
				{
					continue;
				}
			}
			System.Globalization.CultureInfo[] infos = new System.Globalization.CultureInfo[list.Count];
			list.CopyTo(infos);
			return infos;
		}

		/// <summary>
		/// Change selected language and update UI.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnChangeLanguage(object sender, System.EventArgs e)
		{
			string oldLanguage = m_CurrentLanguage;
			try
			{				
				foreach(MenuItem it in LanguageMnu.MenuItems)
				{
					it.Checked = false;
				}
				MenuItem item = (MenuItem) sender; 
				foreach (System.Globalization.CultureInfo it in System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.SpecificCultures))
				{
					string str = it.NativeName;
					int len = str.IndexOf("(");
					if (len > 0)
					{
						str = str.Substring(0, len);
					}
					if (str == item.Text)
					{						
						System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture = it;
						m_CurrentLanguage = it.TwoLetterISOLanguageName;
						UpdateResources();
						break;
					}
				}
				item.Checked = true;
			}
			catch //Restore old language.
			{
				m_CurrentLanguage = oldLanguage;
				System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(m_CurrentLanguage);			
			}
		}
	}
}
