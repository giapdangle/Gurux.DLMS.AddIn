//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXManufacturer.cs $
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
	/// Defines the DLMS manufacturer.
	/// </summary>
	internal class GXManufacturer
	{
		string m_Name = string.Empty;
		GXManufacturerDevice[] m_ManufacturerDevices = null;

		/// <summary>
		/// Initializes a new instance of the GXManufacturer class.
		/// </summary>
		/// <param name="name">The name of the manufacturer.</param>
		/// <param name="devices">An array of GXManufacturerDevices associated with the manufaturer.</param>
		public GXManufacturer(string name, GXManufacturerDevice[] devices)
		{
			m_Name = name;
			m_ManufacturerDevices = devices;
		}

		/// <summary>
		/// Get the name of the manufacturer.
		/// </summary>
		public string ManufacturerName
		{
			get
			{
				return m_Name;
			}
		}

		/// <summary>
		/// Get the collection of manufacturer devices
		/// </summary>
		public GXManufacturerDevice[] ManufacturerDevices
		{
			get
			{
				return m_ManufacturerDevices;
			}
		}

		/// <summary>
		/// Returns a String representation of the value of this instance of the GXManufacturer class.
		/// </summary>
		/// <returns>The name of the manufaturer.</returns>
		public override string ToString()
		{
			return m_Name;
		}

	}
}
