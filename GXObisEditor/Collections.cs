//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXObisEditor/Collections.cs $
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
using System.Collections;

namespace GXObisEditor
{
	/// <summary>
	/// Allection class for ObisDevices.
	/// </summary>
	internal class ObisDevices : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the ObisDevices class.
		/// </summary>
		public ObisDevices()
		{
		}

		public int Add(ObisDevice device)
		{
			return List.Add(device);
		}

		/// <summary>
		/// Inserts an item to the ObisDevices at the specified index.
		/// </summary>
		/// <param name="index">The target index.</param>
		/// <param name="device">The inserted device.</param>
		public void Insert(int index, ObisDevice device)
		{
			List.Insert(index, device);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the ObisDevices.
		/// </summary>
		/// <param name="device">The removed device.</param>
		public void Remove(ObisDevice device)
		{
			List.Remove(device);
		}

		/// <summary>
		/// Determines whether the ObisDevices contains a specific device.
		/// </summary>
		/// <param name="device">The device to locate.</param>
		/// <returns>True if the device was located.</returns>
		public bool Contains(ObisDevice device)
		{
			return List.Contains(device);
		}

		/// <summary>
		/// Returns the zero-based index of the first occurrence of a value in the ObisDevices.
		/// </summary>
		/// <param name="device">The device to locate.</param>
		/// <returns>The index where the device was located or -1 if it was not found.</returns>
		public int IndexOf(ObisDevice device)
		{
			return List.IndexOf(device);
		}

		/// <summary>
		/// Copies the ObisDevices to an System.Array, starting at a particular System.Array index.
		/// </summary>
		/// <param name="array">The array that is the destination of the ObisDevices.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(ObisDevice[] array, int index)
		{
			List.CopyTo(array, index);
		}

		/// <summary>
		/// Retrieves an item from the collection using a numeric index.
		/// </summary>
		/// <param name="index">Zero based index.</param>
		/// <returns>The device.</returns>
		public ObisDevice this[int index]
		{
			get
			{
				return (ObisDevice)List[index];
			}
			set
			{
				List[index] = value;
			}
		}
	}

	/// <summary>
	/// A collection class for ObisItems.
	/// </summary>
	internal class ObisItems : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the ObisItems class.
		/// </summary>
		public ObisItems()
		{
		}

		public int Add(ObisItem Item)
		{
			return List.Add(Item);
		}

		/// <summary>
		/// Inserts an item to the ObisItems at the specified index.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <param name="item">The inserted item.</param>
		public void Insert(int index, ObisItem item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the ObisItems.
		/// </summary>
		/// <param name="item">The removed item.</param>
		public void Remove(ObisItem item)
		{
			List.Remove(item);
		}

		/// <summary>
		/// Determines whether the ObisItems contains a specific value.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <returns>True if the item was located.</returns>
		public bool Contains(ObisItem item)
		{
			return List.Contains(item);
		}

		/// <summary>
		/// Returns the zero-based index of the first occurrence of a value in the ObisItems.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <returns>The index where the item is located or -1 if it was not found.</returns>
		public int IndexOf(ObisItem item)
		{
			return List.IndexOf(item);
		}

		/// <summary>
		/// Copies the ObisItems or a portion of it to an array.
		/// </summary>
		/// <param name="array">The array that is the destination of the ObisItems.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(ObisItem[] array, int index)
		{
			List.CopyTo(array, index);
		}

		/// <summary>
		/// Retrieves an item from the collection using a numeric index.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>The item.</returns>
		public ObisItem this[int index]
		{
			get
			{
				return (ObisItem)List[index];
			}
			set
			{
				List[index] = value;
			}
		}
	}

	/// <summary>
	/// A collection class for ObisInterfaces.
	/// </summary>
	internal class ObisInterfaces : CollectionBase
	{
		/// <summary>
		/// Initializes a new instance of the ObisInterfaces class.
		/// </summary>
		public ObisInterfaces()
		{
		}

		public int Add(int item)
		{
			return List.Add(item);
		}

		/// <summary>
		/// Inserts an item to the ObisInterfaces at the specified index.
		/// </summary>
		/// <param name="index">The target index.</param>
		/// <param name="device">The inserted item.</param>
		public void Insert(int index, int item)
		{
			List.Insert(index, item);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the ObisInterfaces.
		/// </summary>
		/// <param name="item">The removed item.</param>
		public void Remove(int item)
		{
			List.Remove(item);
		}

		/// <summary>
		/// Determines whether the ObisInterfaces contains a specific value.
		/// </summary>
		/// <param name="item">The value to locate.</param>
		/// <returns>True if the item was found.</returns>
		public bool Contains(int item)
		{
			return List.Contains(item);
		}

		/// <summary>
		/// Returns the zero-based index of the first occurrence of a value in the ObisInterfaces.
		/// </summary>
		/// <param name="item">The item to locate.</param>
		/// <returns>The index of the item, or -1 if it was not found.</returns>
		public int IndexOf(int item)
		{
			return List.IndexOf(item);
		}

		/// <summary>
		/// Copies the ObisInterfaces or a portion of it to an array.
		/// </summary>
		/// <param name="array">The array that is the destination of the ObisInterfaces.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(int[] array, int index)
		{
			List.CopyTo(array, index);
		}

		public int[] Array
		{
			get
			{
				int[] retVal = new int[Count];
				this.CopyTo(retVal, 0);
				return retVal;
			}
		}

		/// <summary>
		/// Retrieves an item from the collection using a numeric index.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>The interface.</returns>
		public int this[int index]
		{
			get
			{
				return (int)List[index];
			}
			set
			{
				List[index] = value;
			}
		}
	}
}
