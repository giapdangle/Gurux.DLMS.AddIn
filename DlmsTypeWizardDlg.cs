//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/DlmsTypeWizardDlg.cs $
//
// Version:         $Revision: 4097 $,
//                  $Date: 2011-09-27 01:08:32 +0300 (Tue, 27 Sep 2011) $
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
using System.Globalization;
using Gurux.Device.Editor;
using Gurux.Device;
using Gurux.Common;

namespace Gurux.DLMS.AddIn
{
	/// <summary>
	/// A DLMS specific custom wizard page. The page is used with the GXWizardDlg class.
	/// </summary>
	internal class DlmsTypeWizardDlg : System.Windows.Forms.Form, IGXWizardPage
	{
        private System.Windows.Forms.Label nameLbl;
        private System.Windows.Forms.TextBox nameTb;		
		private System.ComponentModel.Container m_Components = null;
        object Target;
        GXDLMSDevice Device;
		/// <summary>
		/// Initializes a new instance of the DlmsTypeWizardDlg class.
		/// </summary>
        public DlmsTypeWizardDlg(GXDevice device, object target)
		{
            Target = target;
			InitializeComponent();						
			this.TopLevel = false;
			this.FormBorderStyle = FormBorderStyle.None;
            Device = device as GXDLMSDevice;
            UpdateResources();
		}

		private void UpdateResources()
		{
            if (Device.UseLogicalNameReferencing)
            {
                this.nameLbl.Text = Gurux.DLMS.AddIn.Properties.Resources.LogicalNameTxt;
            }
            else
            {
                this.nameLbl.Text = Gurux.DLMS.AddIn.Properties.Resources.ShortNameTxt;
            }
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
            this.nameLbl = new System.Windows.Forms.Label();
            this.nameTb = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // nameLbl
            // 
            this.nameLbl.Location = new System.Drawing.Point(8, 32);
            this.nameLbl.Name = "nameLbl";
            this.nameLbl.Size = new System.Drawing.Size(104, 16);
            this.nameLbl.TabIndex = 0;
            this.nameLbl.Text = "NameLbl";
            // 
            // nameTb
            // 
            this.nameTb.Location = new System.Drawing.Point(120, 32);
            this.nameTb.Name = "nameTb";
            this.nameTb.Size = new System.Drawing.Size(216, 20);
            this.nameTb.TabIndex = 2;
            // 
            // DlmsTypeWizardDlg
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(464, 397);
            this.Controls.Add(this.nameTb);
            this.Controls.Add(this.nameLbl);
            this.Name = "DlmsTypeWizardDlg";
            this.Text = "DlmsTypeWizardDlg";
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region IGXWizardPage Members

		public void Back()
		{
		}

        /// <summary>
        /// Checks the integrity of a logical name.
        /// </summary>
        /// <param name="ln">Logical name under observance.</param>
        /// <returns>Error message if there is one. Empty string is successful result.</returns>
        public static string CheckLogicalName(string[] arr)
        {
            try
            {
                double a = 0, b = 0, c = 0, d = 0, e = 0, f = 0;
                if (arr.Length != 6 ||
                    !double.TryParse(arr[0], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out a) ||
                    !double.TryParse(arr[1], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out b) ||
                    !double.TryParse(arr[3], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out d) ||
                    !double.TryParse(arr[4], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out e) ||
                    !double.TryParse(arr[5], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out f) ||
                    a < 0 || a > 255 ||
                    b < 0 || b > 255 ||
                    d < 0 || d > 255 ||
                    e < 0 || e > 255 ||
                    f < 0 || f > 255)
                {
                    throw new Exception("Logical name is invalid format.");
                }
                if ((!double.TryParse(arr[2], NumberStyles.Number, NumberFormatInfo.CurrentInfo, out c) ||
                    c < 0 || c > 255) &&
                    !(string.Compare(arr[2], "C", true) == 0 ||
                    string.Compare(arr[2], "F", true) == 0 ||
                    string.Compare(arr[2], "L", true) == 0 ||
                    string.Compare(arr[2], "P", true) == 0))
                {
                    throw new Exception("Logical name is invalid format.");
                }
                return string.Empty;
            }
            catch (Exception Ex)
            {
                return Ex.Message;
            }
        }

		public void Next()
		{
            if (Device.UseLogicalNameReferencing)
			{
				string str = nameTb.Text;
				str = str.Replace("(", "");
				str = str.Replace(")", "");
				str = str.Replace("-", ".");
				str = str.Replace(":", ".");
				//LN is Dotted notation.
				string[] arr = str.Split('.');
				if (arr.Length == 5) // If 255 is not given.
				{
					str += ".255";
					arr = str.Split('.');
				}
                if (arr.Length != 6)
                {
                    throw new Exception("Logical name is in invalid format. It must give in dotter format. Eg. 0.0.1.0.0.255");
                }
				string errorText = CheckLogicalName(arr);
				if (errorText != null && errorText.Trim().Length > 0)
				{
					throw new Exception(errorText);
				}
			}
			else
			{
				string str = nameTb.Text.ToLower().Replace("0x", "");
				try
				{
					int sn = int.Parse(str, NumberStyles.HexNumber);
				}
				catch
				{
					throw new Exception("Short name is in invalid format. It must give in hex format. Eg. 0x234");
				}
			}
		}        

		/// <summary>
		/// Gets a string that describes the wizard page object.
		/// </summary>
		public string Description
		{
			get
			{
				return Gurux.DLMS.AddIn.Properties.Resources.TypeWizardDescriptionTxt;
			}
		}

		public string Caption
		{
			get
			{
				return Gurux.DLMS.AddIn.Properties.Resources.TypeWizardHeaderTxt;
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
            GXDLMSProperty prop = Target as GXDLMSProperty;
            GXDLMSTable table = Target as GXDLMSTable;
            GXDLMSCategory category = Target as GXDLMSCategory;
            if (Device.UseLogicalNameReferencing)
            {
                if (prop != null)
                {
                    prop.LogicalName = nameTb.Text;
                }
                else if (category != null)
                {
                    category.LogicalName = nameTb.Text;
                }
                else if (table != null)
                {
                    table.LogicalName = nameTb.Text;
                }
            }
            else
            {
                if (prop != null)
                {
                    prop.ShortName = Convert.ToUInt16(nameTb.Text, 16);
                }
                else if (category != null)
                {
                    category.ShortName = Convert.ToUInt16(nameTb.Text, 16);
                }
                else if (table != null)
                {
                    table.ShortName = Convert.ToInt32(nameTb.Text, 16);
                }
            }
		}

		public void Initialize()
		{
            string str = "";
            if (Target is GXDLMSProperty)
            {
                GXDLMSProperty prop = Target as GXDLMSProperty;
                if (prop.ShortName != 0)
                {
                    str = Convert.ToString(prop.ShortName, 16);
                }
                else
                {
                    str = prop.LogicalName;
                }
            }
            else if (Target is GXDLMSTable)
            {
                GXDLMSTable table = Target as GXDLMSTable;
                if (table.ShortName != 0)
                {
                    str = Convert.ToString(table.ShortName, 16);
                }
                else
                {
                    str = table.LogicalName;
                }
            }
            else if (Target is GXDLMSCategory)
            {
                GXDLMSCategory category = Target as GXDLMSCategory;
                if (category.ShortName != 0)
                {
                    str = Convert.ToString(category.ShortName, 16);
                }
                else
                {
                    str = category.LogicalName;
                }
            }
            nameTb.Text = str;
        }

		public void Cancel()
		{
		}

		private void TypeCb_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			nameTb.ReadOnly = false;
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
	}
}
