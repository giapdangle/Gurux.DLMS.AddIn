//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/UIPropertyEditor.cs $
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
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace DLMS
{
	/// <summary>
	/// Summary description for UIPropertyEditor.
	/// </summary>
	public class UIPropertyEditor : UITypeEditor
	{
		IWindowsFormsEditorService edSvc = null;
		TreeView m_tree = null;
		/// <summary>
		/// Shows a dropdown icon in the property editor
		/// </summary>
		/// <param name="context">The context of the editing control</param>
		/// <returns>Returns <c>UITypeEditorEditStyle.DropDown</c></returns>
		public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context) 
		{
			return UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// Load default images for device tree.
		/// </summary>
		/// <param name="Images"></param>
		void LoadDefaultImages(ImageList.ImageCollection Images)
		{		
			System.Drawing.Bitmap bm = null;
			//Load device categories image
			System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceCategories.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}

			//Load device category image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceCategory.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent(System.Drawing.Color.FromArgb(255, 0, 255));
				Images.Add(bm);
			}

			//Load device tables image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceTables.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}

			//Load device table image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceTable.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}

			//Load device properties image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceProperties.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}

			//Load device property image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DeviceProperty.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}
			//Load None image
			stream = this.GetType().Assembly.GetManifestResourceStream("DLMS.Resources.DatabaseNone.bmp");
			if (stream != null)
			{
				bm = new System.Drawing.Bitmap(stream);
				bm.MakeTransparent();					
				Images.Add(bm);
			}
		}

		/// <summary>
		/// Overrides the method used to provide basic behaviour for selecting editor.
		/// Shows our custom control for editing the value.
		/// </summary>
		/// <param name="context">The context of the editing control</param>
		/// <param name="provider">A valid service provider</param>
		/// <param name="value">The current value of the object to edit</param>
		/// <returns>The new value of the object</returns>
		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value) 
		{
			if (context == null || context.Instance == null || provider == null) 
			{
				return base.EditValue(context, provider, value);
			}
			
			if (m_tree == null)
			{
				m_tree = new TreeView();
				m_tree.BorderStyle = BorderStyle.None;			
				m_tree.ImageList = new ImageList();
				LoadDefaultImages(m_tree.ImageList.Images);
			}
			m_tree.Nodes.Clear();
			GuruxDeviceEditor.GXDevice device = (GuruxDeviceEditor.GXDevice) context.Instance;
			//IGXComponent Comp = (IGXComponent) type.Parent;
			//GuruxDeviceEditor.GXDevice device = (GuruxDeviceEditor.GXDevice) ((System.Drawing.Design.DesignerHost) context.Container).GetService(typeof(GuruxDeviceEditor.GXDevice));
			if ((edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService))) == null) 
			{
				return value;
			}
			// Create a CheckedListBox and populate it with all the propertylist values			
			TreeNode NoneNode = new TreeNode("None");
			NoneNode.ImageIndex = NoneNode.SelectedImageIndex = 6;
			m_tree.Nodes.Add(NoneNode);
			TreeNode parent = m_tree.Nodes.Add("Tables");
			parent.Tag = device.Tables;
			parent.ImageIndex = 2;
			foreach(GuruxDeviceEditor.GXTable table in device.Tables)
			{
				TreeNode node = parent.Nodes.Add(table.Name);
				node.SelectedImageIndex = node.ImageIndex = 3;
				node.Tag = table;
				foreach(GuruxDeviceEditor.GXProperty prop in table.Properties)
				{
					TreeNode item = node.Nodes.Add(prop.Name);
					item.SelectedImageIndex = item.ImageIndex = 5;
					item.Tag = prop;
					if (value == prop)
					{
						m_tree.SelectedNode = item;
						item.EnsureVisible();
					}
				}
			}
			parent = m_tree.Nodes.Add("Categories");
			parent.ImageIndex = 0;
			parent.Tag = device.Categories;
			foreach(GuruxDeviceEditor.GXCategory cat in device.Categories)
			{
				TreeNode node = parent.Nodes.Add(cat.Name);
				node.SelectedImageIndex = node.ImageIndex = 1;				
				node.Tag = cat;
				foreach(GuruxDeviceEditor.GXProperty prop in cat.Properties)
				{
					TreeNode item = node.Nodes.Add(prop.Name);
					item.Tag = prop;
					item.SelectedImageIndex = item.ImageIndex = 5;
					if (value == prop)
					{
						m_tree.SelectedNode = item;
						item.EnsureVisible();
					}
				}
			}
			m_tree.DoubleClick += new System.EventHandler(this.DoubleClick);
			// Show Listbox as a DropDownControl. This methods returns only when the dropdowncontrol is closed
			edSvc.DropDownControl(m_tree);
			if (m_tree.SelectedNode != null)
			{
				if (m_tree.SelectedNode.Tag is GuruxDeviceEditor.GXProperty)
				{
					return m_tree.SelectedNode.Tag;
				}
				if (m_tree.SelectedNode.Tag == null)
				{
					return null;
				}
			}
			return value;
		}

		/// <summary>
		/// Close wnd when user selects new item.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void DoubleClick(object sender, System.EventArgs e)
		{	
			//Do nothing If property or empty item is not selected.
			if (m_tree.SelectedNode != null && !(m_tree.SelectedNode.Tag == null ||
				(m_tree.SelectedNode.Tag is GuruxDeviceEditor.GXProperty)))
			{
				return;
			}
			if (edSvc != null) 
			{
				edSvc.CloseDropDown();
			}
		}
	}
}
