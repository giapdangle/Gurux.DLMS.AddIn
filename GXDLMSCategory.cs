using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurux.Device;
using System.Runtime.Serialization;
using System.ComponentModel;
using Gurux.Device.Editor;

namespace Gurux.DLMS.AddIn
{	
	public class GXDLMSObjectTypeConverter : GXObjectTypeConverter
    {
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection items =  base.GetProperties (context, value, attributes);
			GXDLMSDevice device = context.GetService(typeof(GXDevice)) as GXDLMSDevice;
            if (device == null)
            {
                if (value is GXProperty)
                {
                    device = (value as GXProperty).Device as GXDLMSDevice;
                }
                else if (value is GXCategory)
                {
                    device = (value as GXCategory).Device as GXDLMSDevice;
                }
                else if (value is GXTable)
                {
                    device = (value as GXTable).Device as GXDLMSDevice;
                }
            }
			if (device != null)
			{
				string name;
				if (device.UseLogicalNameReferencing)
				{	
					name = "ShortName";					
				}
				else
				{
					name = "LogicalName";
				}
				foreach (PropertyDescriptor pd in items)
        		{
					if (pd.Name == name)
					{
						items.Remove (pd);
						break;
					}
				}
			}
			return items;
		}
	}

    [DataContract()]
	[TypeConverter(typeof(GXDLMSObjectTypeConverter))]
    public class GXDLMSCategory : GXCategory
    {
        /// <summary>
        /// COSEM object's Short Name.
        /// </summary>
        /// <remarks>
        /// Short name is not shown on the property grid.
        /// </remarks>
        [DataMember(IsRequired = false, EmitDefaultValue = false), DefaultValue(0)]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public UInt16 ShortName
        {
            get;
            set;
        }

        /// <summary>
        /// COSEM object's Logical Name.
        /// </summary>
        /// <remarks>
        /// Short name is not shown on the property grid.
        /// </remarks>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public string LogicalName
        {
            get;
            set;
        }

        protected static void UpdateBaudrates(GXDLMSProperty prop)
        {
            prop.Values.Clear();
            prop.Values.Add(new GXValueItem("300", 0));
            prop.Values.Add(new GXValueItem("600", 1));
            prop.Values.Add(new GXValueItem("1200", 2));
            prop.Values.Add(new GXValueItem("2400", 3));
            prop.Values.Add(new GXValueItem("4800", 4));
            prop.Values.Add(new GXValueItem("9600", 5));
            prop.Values.Add(new GXValueItem("19200", 6));
            prop.Values.Add(new GXValueItem("38400", 7));
            prop.Values.Add(new GXValueItem("57600", 8));
            prop.Values.Add(new GXValueItem("115200", 9));
            prop.ForcePresetValues = true;
            prop.DefaultValue = "300";
        }

        public override void Validate(bool designMode, GXTaskCollection tasks)
		{
            if (designMode)
            {
                GXDLMSDevice device = this.Device as GXDLMSDevice;
                if (device == null)//GXDeviceEditor uses this.
                {
                    device = this.Site.GetService(typeof(GXDevice)) as GXDLMSDevice;
                }
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
                            tasks.Add(new GXTask(this, "ShortName", "Short name is unknown."));
                        }
                    }
                }
            }
            base.Validate(designMode, tasks);			
		}
    }
}
