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
    public class GXDLMSHdlcSetup : GXDLMSCategory
    {
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.ExtendedRegister;
            }
        }

        public override void Initialize(object info)
        {
            GXObisCode code = info as GXObisCode;
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Logical Name", 1));
            GXDLMSProperty p = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Communication speed", 2);
            GXDLMSIECOpticalPortSetup.UpdateBaudrates(p);
            this.Properties.Add(p);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Window size in transmit", 3));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Window size in receive", 4));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Maximum info length transmit", 5));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Maximum info length receive", 6));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Inter charachter timeout", 7));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Inactivity timeout", 8));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Device Address", 9));
        }
    }
}
