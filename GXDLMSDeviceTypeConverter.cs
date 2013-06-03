using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    class GXDLMSDeviceTypeConverter : Gurux.Device.Editor.GXObjectTypeConverter
    {
        /// <summary>
        /// Hide base properties.
        /// </summary>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            GXDLMSDevice device = value as GXDLMSDevice;
            PropertyDescriptorCollection props = base.GetProperties(context, value, attributes);
            PropertyDescriptorCollection pdc = new PropertyDescriptorCollection(null);
            if (device != null)
            {
                bool useSerial = device.HDLCAddressing == HDLCAddressType.SerialNumber;
                foreach (PropertyDescriptor it in props)
                {
                    if (it.Name == "SerialNumber" && !useSerial)
                    {
                        continue;
                    }
                    if (it.Name == "PhysicalAddress" && useSerial)
                    {
                        continue;
                    }
                    pdc.Add(it);
                }
                return pdc;
            }
            return props;
        }
    }
}
