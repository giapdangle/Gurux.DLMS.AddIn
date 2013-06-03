//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/DataHolders.cs $
//
// Version:         $Revision: 2752 $,
//                  $Date: 2010-09-22 14:40:33 +0300 (ke, 22 syys 2010) $
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
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace GXObisEditor
{
	/// <summary>
	/// Defines an OBIS manufacturer.
	/// </summary>
	internal class ObisManufacturer
	{
		string m_Name = string.Empty;
		int m_Type = 0;
		string m_Description = string.Empty;
		ObisDevices m_Devices = new ObisDevices();
		ObisItems m_CommonProperties = new ObisItems();
		string m_FilePath = string.Empty;

		/// <summary>
		/// Initializes a new instance of the ObisManufacturer class.
		/// </summary>
		/// <param name="name">The name of the manufacturer.</param>
		/// <param name="type">The type of the manufacturer</param>
		/// <param name="description">The description of the manufacturer.</param>
		public ObisManufacturer(string name, int type, string description)
		{
			this.Name = name;
			this.Type = type;
			this.Description = description;
		}

		/// <summary>
		/// Initializes a new instance of the ObisManufacturer class.
		/// </summary>
		public ObisManufacturer()
		{
		}

		/// <summary>
		/// Returns a String representation of the value of this instance of the ObisManufacturer class.
		/// </summary>
		/// <returns>The name of the manufacturer.</returns>
		public override string ToString()
		{
			return m_Name;
		}

		public void Load(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
				{
					throw new Exception("File does not exist!");
				}
				LoadXmlFile(filePath);
				this.FilePath = filePath;
			}
			catch (Exception Ex)
			{
				MessageBox.Show("Error occurred during file load: " + Ex.Message);
			}
		}

		public void Save(string filePath)
		{
			SaveXmlFile(filePath);
			this.FilePath = filePath;
		}

		private void SaveXmlFile(string filePath)
		{
			XmlTextWriter textWriter = new XmlTextWriter(filePath, null);
			try
			{
				textWriter.Formatting = Formatting.Indented;
				//textWriter.WriteStartDocument();
				// Write first element

				textWriter.WriteStartElement("Manufacturer");
				textWriter.WriteAttributeString("Name", Name);
				textWriter.WriteAttributeString("Type", Type.ToString());
				textWriter.WriteAttributeString("Description", Description);

				textWriter.WriteStartElement("CommonProperties");
				foreach (ObisItem item in CommonProperties)
				{
					WriteXmlItem(item, textWriter);
				}
				textWriter.WriteEndElement();

				foreach (ObisDevice dev in Devices)
				{
					textWriter.WriteStartElement("Device");
					textWriter.WriteAttributeString("Name", dev.Name);
					textWriter.WriteAttributeString("Description", dev.Description);
					textWriter.WriteAttributeString("Type", dev.Type.ToString());
					textWriter.WriteAttributeString("PrimaryDefaultID", dev.PrimaryDefaultID.ToString());
					textWriter.WriteAttributeString("PrimaryDefaultIDSize", dev.PrimaryDefaultIDSize.ToString());
					textWriter.WriteAttributeString("PrimaryLowID", dev.PrimaryLowID.ToString());
					textWriter.WriteAttributeString("PrimaryLowIDSize", dev.PrimaryLowIDSize.ToString());
					textWriter.WriteAttributeString("PrimaryHighID", dev.PrimaryHighID.ToString());
					textWriter.WriteAttributeString("PrimaryHighIDSize", dev.PrimaryHighIDSize.ToString());
					textWriter.WriteAttributeString("SecondaryID", dev.SecondaryID.ToString());
					textWriter.WriteAttributeString("SecondaryIDSize", dev.SecondaryIDSize.ToString());
					textWriter.WriteAttributeString("UseLN", Convert.ToInt16(dev.UseLN).ToString());
					textWriter.WriteAttributeString("SupportNetworkSpecificSettings", Convert.ToInt16(dev.SupportNetworkSpecificSettings).ToString());
					foreach (ObisItem item in dev.Items)
					{
						WriteXmlItem(item, textWriter);
					}
					textWriter.WriteEndElement();
				}

				//end Manufacturer
				textWriter.WriteEndElement();
				//textWriter.WriteEndDocument();
			}
			finally
			{
				textWriter.Close();
				GuruxCommon.GXCommon.UpdateFileSecurity(filePath);
			}
		}

		private void WriteXmlItem(ObisItem item, XmlTextWriter textWriter)
		{
			textWriter.WriteStartElement("obis");
			textWriter.WriteAttributeString("LN", item.LN);
			textWriter.WriteAttributeString("Attribute", item.Attribute.ToString());
			textWriter.WriteAttributeString("Type", item.Type.ToString());
			textWriter.WriteAttributeString("Description", item.Description);
			foreach (int iface in item.Interfaces)
			{
				textWriter.WriteStartElement("Interface");
				textWriter.WriteAttributeString("Type", iface.ToString());
				textWriter.WriteEndElement();
			}
			textWriter.WriteStartElement("Values");
			foreach (Gurux.DeviceEditor.GXValueItem val in item.ValueItems)
			{
				textWriter.WriteStartElement("Value");
				textWriter.WriteAttributeString("Device", val.Value);
				textWriter.WriteString(val.UIValue);
				textWriter.WriteEndElement();//<Value>
			}
			textWriter.WriteEndElement();//<Values>
			textWriter.WriteEndElement();
		}

		private void LoadXmlFile(string filePath)
		{
			XmlTextReader reader = new XmlTextReader(filePath);
			try
			{
				XmlDocument doc = new XmlDocument();
				XmlNode rootNode = doc.ReadNode(reader);
				if (rootNode.Name != "Manufacturer")
				{
					throw new Exception("Incorret OBIS file");
				}
				m_Name = rootNode.Attributes["Name"].Value;
				m_Type = int.Parse(rootNode.Attributes["Type"].Value);
				m_Description = rootNode.Attributes["Description"].Value;

				if (rootNode.ChildNodes.Count > 1)
				{
					for (int i = 0; i < rootNode.ChildNodes.Count; ++i)
					{
						XmlNode node = rootNode.ChildNodes[i];
						//Whitespace check
						if (node is XmlWhitespace)
						{
							continue;
						}
						//Common Properies
						else if (node.Name == "CommonProperties")
						{
							XmlNode common = rootNode.ChildNodes[i];
							foreach (XmlNode item in common.ChildNodes)
							{
								if (item is XmlElement)
								{
									m_CommonProperties.Add(ParseObisItem(item));
								}
							}

						}
						else if (node.Name == "Device")
						{
							ObisDevice obde = new ObisDevice();
							obde.Name = node.Attributes["Name"].Value;
							obde.Description = node.Attributes["Description"].Value;
							obde.Type = int.Parse(node.Attributes["Type"].Value);
							try
							{
								obde.PrimaryDefaultID = int.Parse(node.Attributes["PrimaryDefaultID"].Value);
								obde.PrimaryDefaultIDSize = int.Parse(node.Attributes["PrimaryDefaultIDSize"].Value);
								obde.PrimaryLowID = int.Parse(node.Attributes["PrimaryLowID"].Value);
								obde.PrimaryLowIDSize = int.Parse(node.Attributes["PrimaryLowIDSize"].Value);
								obde.PrimaryHighID = int.Parse(node.Attributes["PrimaryHighID"].Value);
								obde.PrimaryHighIDSize = int.Parse(node.Attributes["PrimaryHighIDSize"].Value);
							}
							catch
							{
								MessageBox.Show("Primary ID is not set for all Access Levels");
							}
							obde.SecondaryID = int.Parse(node.Attributes["SecondaryID"].Value);
							obde.SecondaryIDSize = int.Parse(node.Attributes["SecondaryIDSize"].Value);
							obde.UseLN = Convert.ToBoolean(int.Parse(node.Attributes["UseLN"].Value));
							System.Xml.XmlAttribute att = node.Attributes["SupportNetworkSpecificSettings"];
							if (att != null)
							{
								obde.SupportNetworkSpecificSettings = Convert.ToBoolean(int.Parse(att.Value));
							}
							foreach (XmlNode item in node.ChildNodes)
							{
								if (item is XmlElement)
								{
									obde.Items.Add(ParseObisItem(item));
								}
							}
							m_Devices.Add(obde);
						}
					}
				}
			}
			finally
			{
				reader.Close();
			}
		}

		private ObisItem ParseObisItem(XmlNode obis)
		{
			ObisItem obit = new ObisItem();
			obit.LN = obis.Attributes["LN"].Value;
			XmlNode node = obis.Attributes["Attribute"];
			if (node != null)
			{
				obit.Attribute = int.Parse(node.Value);
			}
			obit.Type = int.Parse(obis.Attributes["Type"].Value);
			obit.Description = obis.Attributes["Description"].Value;
			foreach (XmlNode node2 in obis.ChildNodes)
			{
				if (node2 is XmlElement)
				{
					if (node2.Name == "Interface")
					{
						obit.Interfaces.Add(int.Parse(node2.Attributes["Type"].Value));
					}
					else if (node2.Name == "Values")
					{
						foreach (XmlNode node3 in node2.ChildNodes)
						{
							if (node3 is XmlElement)
							{
								obit.ValueItems.Add(new Gurux.DeviceEditor.GXValueItem(node3.Attributes["Device"].Value, node3.InnerText, -1));
							}
						}
					}
				}
			}
			return obit;
		}

		public string FilePath
		{
			get
			{
				return m_FilePath;
			}
			set
			{
				if (value.IndexOfAny(Path.GetInvalidPathChars()) != -1)
				{
					throw new Exception("FilePath contains invalid characters.");
				}
				m_FilePath = value;
			}
		}

		/// <summary>
		/// Gets or sets the name of the OBIS manufacturer object.
		/// </summary>
		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				if (value == null || value.Trim().Length == 0)
				{
					throw new Exception("Name can not be empty");
				}
				m_Name = value;
			}
		}

		public int Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		/// <summary>
		/// Gets or sets a string that describes the OBIS manufacturer object.
		/// </summary>
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
			}
		}

		public ObisDevices Devices
		{
			get
			{
				return m_Devices;
			}
		}

		public ObisItems CommonProperties
		{
			get
			{
				return m_CommonProperties;
			}
		}
	}

	/// <summary>
	/// Defines an OBIS device.
	/// </summary>
	internal class ObisDevice
	{
		string m_Name = string.Empty;
		string m_Description = string.Empty;
		int m_Type = 0;
		int m_PrimaryDefaultID = 0;
		int m_PrimaryDefaultIDSize = 0;
		int m_PrimaryHighID = 0;
		int m_PrimaryHighIDSize = 0;
		int m_PrimaryLowID = 0;
		int m_PrimaryLowIDSize = 0;
		int m_SecondaryID = 0;
		int m_SecondaryIDSize = 0;
		bool m_UseLN = true;
		bool m_SupportNetworkSpecificSettings = false;
		ObisItems m_Items = new ObisItems();

		/// <summary>
		/// Initializes a new instance of the ObisDevice class.
		/// </summary>
		/// <param name="name">The name of the device.</param>
		public ObisDevice(string name)
		{
			this.Name = name;
			this.Description = Description;
		}

		/// <summary>
		/// Initializes a new instance of the ObisDevice class.
		/// </summary>
		public ObisDevice()
		{
		}

		/// <summary>
		/// Returns a String representation of the value of this instance of the ObisDevice class.
		/// </summary>
		/// <returns>The name of the device.</returns>
		public override string ToString()
		{
			return m_Name;
		}

		/// <summary>
		/// Gets or sets the name of the OBIS device.
		/// </summary>
		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				if (value == null || value.Trim().Length == 0)
				{
					throw new Exception("Name can not be empty");
				}
				m_Name = value;
			}
		}

		/// <summary>
		/// Gets or sets a string that describes the OBIS device object.
		/// </summary>
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
			}
		}

		public int Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public int PrimaryDefaultID
		{
			get
			{
				return m_PrimaryDefaultID;
			}
			set
			{
				m_PrimaryDefaultID = value;
			}
		}

		public int PrimaryDefaultIDSize
		{
			get
			{
				return m_PrimaryDefaultIDSize;
			}
			set
			{
				m_PrimaryDefaultIDSize = value;
			}
		}

		public int PrimaryLowID
		{
			get
			{
				return m_PrimaryLowID;
			}
			set
			{
				m_PrimaryLowID = value;
			}
		}

		public int PrimaryLowIDSize
		{
			get
			{
				return m_PrimaryLowIDSize;
			}
			set
			{
				m_PrimaryLowIDSize = value;
			}
		}

		public int PrimaryHighID
		{
			get
			{
				return m_PrimaryHighID;
			}
			set
			{
				m_PrimaryHighID = value;
			}
		}

		public int PrimaryHighIDSize
		{
			get
			{
				return m_PrimaryHighIDSize;
			}
			set
			{
				m_PrimaryHighIDSize = value;
			}
		}

		public int SecondaryID
		{
			get
			{
				return m_SecondaryID;
			}
			set
			{
				m_SecondaryID = value;
			}
		}

		public int SecondaryIDSize
		{
			get
			{
				return m_SecondaryIDSize;
			}
			set
			{
				m_SecondaryIDSize = value;
			}
		}

		public bool UseLN
		{
			get
			{
				return m_UseLN;
			}
			set
			{
				m_UseLN = value;
			}
		}

		public bool SupportNetworkSpecificSettings
		{
			get
			{
				return m_SupportNetworkSpecificSettings;
			}
			set
			{
				m_SupportNetworkSpecificSettings = value;
			}
		}

		public ObisItems Items
		{
			get
			{
				return m_Items;
			}
		}

	}

	/// <summary>
	/// Defines an OBIS item.
	/// </summary>
	internal class ObisItem
	{
		int m_Attribute = -1;
		string m_LN = string.Empty;
		int m_Type = 0;
		string m_Description = string.Empty;
		ObisInterfaces m_Interfaces = new ObisInterfaces();
		public Gurux.DeviceEditor.GXValueItems ValueItems = new Gurux.DeviceEditor.GXValueItems();

		public Gurux.DLMS2.IOBISValueCollection GetValueItems()
		{
			Gurux.DLMS2.IOBISValueCollection vals = new Gurux.DLMS2.COBISValueCollectionClass();
			foreach (Gurux.DeviceEditor.GXValueItem it in ValueItems)
			{
				Gurux.DLMS2.IOBISValue val = new Gurux.DLMS2.COBISValueClass();
				val.DeviceValue = it.Value;
				val.UIValue = it.UIValue;
				vals.Add(val);
			}
			return vals;
		}

		/// <summary>
		/// Initializes a new instance of the ObisItem class.
		/// </summary>
		/// <param name="ln">The logical name of the item.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="description">The description of the item.</param>
		public ObisItem(string ln, int type, string description)
		{
			this.LN = ln;
			this.Type = type;
			this.Description = description;
		}

		/// <summary>
		/// Initializes a new instance of the ObisItem class.
		/// </summary>
		public ObisItem()
		{
		}

		public int Attribute
		{
			get
			{
				return m_Attribute;
			}
			set
			{
				m_Attribute = value;
			}
		}

		public string LN
		{
			get
			{
				return m_LN;
			}
			set
			{
				if (value == null || value.Trim().Length == 0)
				{
					throw new Exception("LN can not be empty");
				}
				m_LN = value;
			}
		}

		public int Type
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		/// <summary>
		/// Gets or sets a string that describes the OBIS item.
		/// </summary>
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
			}
		}

		public ObisInterfaces Interfaces
		{
			get
			{
				return m_Interfaces;
			}
		}
	}
}