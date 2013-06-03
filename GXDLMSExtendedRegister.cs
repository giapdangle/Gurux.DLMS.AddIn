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
    public class GXDLMSExtendedRegister : GXDLMSCategory
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
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Value", 2));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Status", 4));
            GXDLMSProperty prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Capture Time", 5);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);
        }
    }

}
