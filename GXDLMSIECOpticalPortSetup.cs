using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Gurux.Device.Editor;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using System.Runtime.Serialization;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    [DataContract()]
    public class GXDLMSIECOpticalPortSetup : GXDLMSCategory
    {
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.IecLocalPortSetup;
            }
        }

        public override void Initialize(object info)
        {
            GXObisCode code = info as GXObisCode;
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Logical Name", 1));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Defaultmode", 2));
            GXDLMSProperty p = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Default baudrate", 3);
            UpdateBaudrates(p);
            p.DefaultValue = "300";
            this.Properties.Add(p);
            p = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Maximum baudrate", 4);
            UpdateBaudrates(p);
            p.DefaultValue = "300";
            this.Properties.Add(p);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Responsetime", 5));
            GXDLMSProperty prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Device Address", 6);
            prop.ValueType = typeof(string);
            this.Properties.Add(prop);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Password 1", 7));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Password 2", 8));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Password 5", 9));
        }
    }
}
