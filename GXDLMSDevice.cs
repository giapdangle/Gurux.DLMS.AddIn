//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXDLMSDevice.cs $
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
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Gurux.DLMS;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using Gurux.Device.Editor;
using Gurux.Device;
using System.Runtime.Serialization;
using Gurux.Communication;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
	/// <summary>
	/// Extends GXDevice class with the DLMS specific properties.
	/// </summary>    
    [DataContract()]
    [GXInitialActionMessage(InitialActionType.Connected, "InitRead", Index = 1)]
    [GXInitialActionMessage(InitialActionType.Connected, "ReadSNRMSend", "UAReply", Index = 2)]
    [GXInitialActionMessage(InitialActionType.Connected, "ReadAARQ", "AAREReply", "IsAARQSend", "ReadAARQNext", Index = 3)]
    [GXInitialActionMessage(InitialActionType.Disconnecting, "DisconnectRequest", "CheckReplyPacket")]
    //TODO: [GXInitialActionMessage(InitialActionType.KeepAlive, "KeepAliveRequest", "CheckReplyPacket")]
    [TypeConverter(typeof(GXDLMSDeviceTypeConverter))]    
    public class GXDLMSDevice : GXDevice
	{
		/// <summary>
		/// Initializes a new instance of the GXDLMSDevice class.
		/// </summary>
		public GXDLMSDevice()
		{            
            LogicalAddress = 0;
            this.StartProtocol = StartProtocolType.IEC;
            this.HDLCAddressing = HDLCAddressType.Default;
            this.GXClient.ByteOrder = ByteOrder.BigEndian;
            this.GXClient.Eop = this.GXClient.Bop = (byte)0x7e;
            this.GXClient.ChecksumSettings.Type = Gurux.Communication.ChecksumType.Fcs16;
            this.GXClient.ChecksumSettings.Position = -2;		                            
            this.GXClient.ChecksumSettings.Start = 1;
            this.GXClient.ChecksumSettings.Count = -2;            
            UseRemoteSerial = false;
            this.WaitTime = 10000;
			AllowedMediaTypes.Add(new GXMediaType("Serial", ""));
			AllowedMediaTypes.Add(new GXMediaType("Net", ""));
			AllowedMediaTypes.Add(new GXMediaType("Terminal", ""));
		}

		/// <summary>
		/// Is device identifier checked on connect.
		/// </summary>
        [DefaultValue(false),
        Category("Device Identification"),
        Description("Is device identifier checked on connect.")]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [Browsable(false)]
        public bool UseDeviceIdentification
        {
            get;
            set;
        }
				
        /// <summary>
        /// Is IEC standard 62056-47 used when communicating using TCP/IP networks.
        /// </summary>
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        [DefaultValue(false), Category("Design"),
        Description("Is IEC standard 62056-47 used when communicating using TCP/IP networks.")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public bool SupportNetworkSpecificSettings
        {
            get;
            set;
        }			
		
		/// <summary>
		/// Is device clock checked on connect
		/// </summary>
		[DefaultValue(false),
		Category("Device Clock"),
		Description("Is device clock checked on connect")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.None)]
        [Browsable(false)]
        public bool CheckDeviceClock
		{
            get;
            set;
		}

		/// <summary>
		/// Minimum time difference needed to synchronize device clock (d.hh:mm:ss)
		/// </summary>
        [DefaultValue("PT0S"),
		Category("Device Clock"),
		Description("Minimum time difference needed to synchronize device clock (d.hh:mm:ss)")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.None)]
        [Browsable(false)]
        public TimeSpan MinimumDeviceClockDifference
		{
            get;
            set;
		}

		/// <summary>
		/// Maximum time difference when to synchronize device clock (d.hh:mm:ss)
		/// </summary>
        [DefaultValue("PT0S"),
		Category("Device Clock"),
		Description("Maximum time difference when to synchronize device clock (d.hh:mm:ss)")]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.None)]
        [Browsable(false)]
        public TimeSpan MaximumDeviceClockDifference
		{
            get;
            set;
		}		

		/// <summary>
		/// Default authentication used when connection is made.
		/// </summary>
        [DefaultValue(Gurux.DLMS.Authentication.None),
		Category("Design"),
		Description("Default authentication used when connection is made.")]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public Gurux.DLMS.Authentication Authentication
		{
            get;
            set;
		}

        /// <summary>
        /// StartProtocol
        /// </summary>
        [DefaultValue(StartProtocolType.IEC), Category("Design")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        public StartProtocolType StartProtocol
        {
            get;
            set;
        }

        /// <summary>
        /// HDLCAddressing
        /// </summary>        
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        [DefaultValue(HDLCAddressType.Default), Category("Design")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public HDLCAddressType HDLCAddressing
        {
            get;            
            set;
        }

        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        [DefaultValue(null), Category("Design")]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string Password
        {
            get;
            set;
        }

        object m_PhysicalAddress;

        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        [Category("Design")]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public object PhysicalAddress
		{
            get
            {
                if (m_PhysicalAddress is string)
                {
                    if (PhysicalAddressSize == 1)
                    {
                        m_PhysicalAddress = Convert.ToByte(m_PhysicalAddress);
                    }
                    else if (PhysicalAddressSize == 2)
                    {
                        m_PhysicalAddress = Convert.ToUInt16(m_PhysicalAddress);

                    }
                    else if (PhysicalAddressSize == 4)
                    {
                        m_PhysicalAddress = Convert.ToUInt32(m_PhysicalAddress);
                    }
                }
                return m_PhysicalAddress;
            }
            set
            {
                m_PhysicalAddress = value;
            }
		}

        /// <summary>
        /// Size of physical address.
        /// </summary>
        [Category("Design")]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int PhysicalAddressSize
        {
            get;
            set;
        }

        [Browsable(false)]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [DefaultValue("")]
        public string SNFormula
        {
            get;
            set;
        }

        [DefaultValue(null), DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        public string SerialNumber
        {
            get;
            set;
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [DefaultValue(0)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        public int LogicalAddress
        {
            get;
            set;
        }

        [DefaultValue(false)]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.Edit)]
        public bool UseRemoteSerial
        {
            get;
            set;
        }        

        [ReadOnly(true)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public string Identification
		{
            get;
            set;
		}		

		[DefaultValue(""), Category("Design"), ReadOnly(true)]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public string ManufacturerName
		{
            get;
            set;
		}

        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public GXManufacturerCollection Manufacturers
        {
            get;
            set;
        }      		

		[Category("Design")]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        public bool UseLogicalNameReferencing
		{
            get;
            set;
		}

        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public object ClientID
        {
            get;
            set;
        }

        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public object ClientIDLow
        {
            get;
            set;
        }

        [DataMember(IsRequired=false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public object ClientIDHigh
        {
            get;
            set;
        }

        public override void Validate(bool designMode, GXTaskCollection tasks)
		{
			if (Convert.ToInt32(ClientID) == 0)
			{
				tasks.Add(new GXTask(this, "ClientID", "Client ID is unknown."));
			}
			if (Convert.ToInt32(PhysicalAddress) == 0)
			{
                tasks.Add(new GXTask(this, "PhysicalAddress", "Physical Address is unknown."));
			}
            base.Validate(designMode, tasks);
		}
	}
}
