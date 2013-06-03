using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurux.Device.Editor;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using System.Runtime.Serialization;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    [DataContract()]
    public class GXDLMSDemandRegister : GXDLMSCategory
    {
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.DemandRegister;
            }
        }

        public override void Initialize(object info)
        {
            GXObisCode code = info as GXObisCode;
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Logical Name", 1));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Current Average Value", 2));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Last Average Value", 3));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Status", 5));
            GXDLMSProperty prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Capture Time", 6);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);
            prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Start Time Current", 7);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Period", 8));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Number of Periods", 9));
        }
    }
}
