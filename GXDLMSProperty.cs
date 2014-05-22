//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXDLMSProperty.cs $
//
// Version:         $Revision: 4118 $,
//                  $Date: 2011-09-27 18:17:06 +0300 (Tue, 27 Sep 2011) $
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gurux.DLMS;
using Gurux.Device.Editor;
using Gurux.Device;
using System.Runtime.Serialization;

namespace Gurux.DLMS.AddIn
{
	/// <summary>
	/// Extends GXProperty class with the DLMS specific properties.
	/// </summary>
	[System.ComponentModel.DefaultProperty("DLMS")]
    [GXReadMessage("ReadData", "ReadDataReply", "IsAllDataReceived", "ReadNext")]
    [GXWriteMessage("ReadDataType", "ReadDataTypeReply", Index = 1)]
    [GXWriteMessage("WriteData", "WriteDataReply", Index = 2)]
    [ToolboxItem(false)]
    [DataContract()]    
	[TypeConverter(typeof(GXDLMSObjectTypeConverter))]
    public class GXDLMSProperty : GXProperty
	{
		/// <summary>
		/// The data type of the property. This data type is described in the template file.
		/// </summary>
		[System.ComponentModel.Category("Design"),
        System.ComponentModel.DefaultValue(DataType.String), System.ComponentModel.Description("Data type of the property.")]
        [ReadOnly(true), ValueAccess(ValueAccessType.Edit, ValueAccessType.None)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public DataType DLMSType
		{
            get;
            set;
		}

        /// <summary>
        /// The UI data type of the property, described in the template file.
        /// </summary>
        /// <remarks>
        /// UIDataType handles serialization.
        /// </remarks>
        [System.ComponentModel.Category("Design"), DefaultValue(null),
        System.ComponentModel.Description("The UI data type of the property, described in the template file.")]
        [ValueAccess(ValueAccessType.Edit, ValueAccessType.None)]
        [Editor(typeof(GXValueTypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [System.Xml.Serialization.XmlIgnore()]
        new public DataType ValueType
        {
            get
            {
                return GXDLMS.Common.GXHelpers.GetDLMSDataType(base.ValueType);
            }
            set
            {
                base.ValueType = GXDLMS.Common.GXHelpers.GetDataType(value);
            }
        }

		/// <summary>
		/// LN Index where data is read.
		/// </summary>        
		[System.ComponentModel.Category("Design"),
        System.ComponentModel.DefaultValue(1),
        System.ComponentModel.Description("Attribute ordinal Index where data is read.")]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public int AttributeOrdinal
		{
            get;
            set;
		}		

		/// <summary>
		/// Initializes a new instance of the GXDLMSProperty class.
		/// </summary>
		public GXDLMSProperty()
		{
            AttributeOrdinal = 1;             
		}

        public GXDLMSProperty(ObjectType type, string ln, UInt16 sn, string name, int attributeOrdinal)
        {
            this.ObjectType = type;
            this.LogicalName = ln;
            this.ShortName = sn;
            AttributeOrdinal = attributeOrdinal;
            this.Name = name;
        }

        public GXDLMSProperty(ObjectType type, string ln, UInt16 sn, string name, int attributeOrdinal, DataType dataType) :
            this(type, ln, sn, name, attributeOrdinal)
        {
            this.DLMSType = dataType;
        }

		
		/// <summary>
		/// Version.
		/// </summary>
        [DefaultValue(0), System.ComponentModel.Category("OBIS"), System.ComponentModel.Description("Version")]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        public int Version
		{
            get;
            set;
		}
		

		/// <summary>
		/// Logical name (LN)
		/// </summary>
		[System.ComponentModel.Category("OBIS"), System.ComponentModel.Description("Logical name (LN)")]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public string LogicalName
		{
            get;
            set;
		}

		/// <summary>
		/// Short name (SN)
		/// </summary>
        [DefaultValue(0), System.ComponentModel.Category("OBIS"), System.ComponentModel.Description("Short name (SN)")]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public UInt16 ShortName
		{
            get;
            set;
		}

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public virtual Gurux.DLMS.ObjectType ObjectType
        {
            get;
            set;
        }

        public override void Validate(bool designMode, GXTaskCollection tasks)
		{		
			GXDLMSDevice device = this.Device as GXDLMSDevice;
			if (device != null)
			{
				if (device.UseLogicalNameReferencing)
				{
					if (string.IsNullOrEmpty(LogicalName))
		            {
		                tasks.Add(new GXTask(this, "LogicalName", "Logical Name is unknown."));
		            }
				}
				else
				{
					if (ShortName == 0)
		            {
                        if (this.Parent == null || !(this.Parent.Parent is GXDLMSTable))
                        {
                            tasks.Add(new GXTask(this, "ShortName", "Short name is unknown."));
                        }
		            }
				}
			}
            base.Validate(designMode, tasks);			
		}
	}
}
