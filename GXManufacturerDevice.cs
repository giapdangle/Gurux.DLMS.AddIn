//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXManufacturerDevice.cs $
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

namespace DLMS
{
	/// <summary>
	/// Defines the DLMS manufacturer device.
	/// </summary>
	internal class GXManufacturerDevice
	{
		public int ManufacturerType = 0;
		string m_Name = string.Empty;
		int m_ClientIDByteSize = 0;
		int m_ServerIDByteSize = 0;
		object m_ClientID = null;
		object m_ServerID = null;

		/// <summary>
		/// Initializes a new instance of the GXManufacturerDevice class.
		/// </summary>
		public GXManufacturerDevice(string name, object clientID, object serverID, int manufacturerType)
		{
			ManufacturerType = manufacturerType;
			m_Name = name;
			m_ClientID = clientID;
			m_ServerID = serverID;

			if (clientID is byte)
			{
				m_ClientIDByteSize = 1;
			}
			else if (clientID is UInt16)
			{
				m_ClientIDByteSize = 2;
			}
			else if (clientID is UInt32)
			{
				m_ClientIDByteSize = 4;
			}

			if (serverID is byte)
			{
				m_ServerIDByteSize = 1;
			}
			else if (serverID is UInt16)
			{
				m_ServerIDByteSize = 2;
			}
			else if (serverID is UInt32)
			{
				m_ServerIDByteSize = 4;
			}
		}

		public string ManufacturerDeviceName
		{
			get
			{
				return m_Name;
			}
		}

		/// <summary>
		/// Client identifier
		/// </summary>
		public object ClientID
		{
			get
			{
				return m_ClientID;
			}
		}

		/// <summary>
		/// The size of the client identifier in bytes.
		/// </summary>
		public int ClientIDByteSize
		{
			get
			{
				return m_ClientIDByteSize;
			}
		}

		/// <summary>
		/// Server identifier
		/// </summary>
		public object ServerID
		{
			get
			{
				return m_ServerID;
			}
		}

		/// <summary>
		/// The size of the server identifier in bytes.
		/// </summary>
		public int ServerIDByteSize
		{
			get
			{
				return m_ServerIDByteSize;
			}
		}

		/// <summary>
		/// Returns a String representation of the value of this instance of the GXManufacturerDevice class.
		/// </summary>
		/// <returns>The name of the device.</returns>
		public override string ToString()
		{
			return m_Name;
		}

	}
}
