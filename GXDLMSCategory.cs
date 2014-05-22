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

        [DataMember(IsRequired = false, EmitDefaultValue = false), DefaultValue(0)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.None)]
        public virtual ObjectType ObjectType
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
