using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurux.Communication;
using System.ComponentModel;
using Gurux.Device.Editor;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using System.Runtime.Serialization;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    [DataContract()]
    public class GXDLMClock : GXDLMSCategory    
    {
        [Browsable(false)]
        public Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.Clock;
            }
        }

        void UpdateStatusValueItems(GXDLMSProperty prop)
        {
            prop.Values.Clear();
            prop.Values.Add(new GXValueItem("OK", 0x0));
            prop.Values.Add(new GXValueItem("Invalid value", 0x1));
            prop.Values.Add(new GXValueItem("Doubtful value", 0x2));
            prop.Values.Add(new GXValueItem("Different Clock Base", 0x4));
            prop.Values.Add(new GXValueItem("Reserved 1", 0x8));
            prop.Values.Add(new GXValueItem("Reserved 2", 0x10));
            prop.Values.Add(new GXValueItem("Reserved 3", 0x20));
            prop.Values.Add(new GXValueItem("Reserved 4", 0x40));
            prop.Values.Add(new GXValueItem("Daylight Save Active", 0x80));
            prop.ForcePresetValues = true;
            prop.DefaultValue = "OK";
            prop.AccessMode = Gurux.Device.AccessMode.Read;
        }

        void UpdateClockBaseValueItems(GXDLMSProperty prop)
        {
            prop.Values.Clear();
            prop.Values.Add(new GXValueItem("Not defined", 0x0));
            prop.Values.Add(new GXValueItem("Internal crystal", 1));
            prop.Values.Add(new GXValueItem("mains frequency 50 Hz", 2));
            prop.Values.Add(new GXValueItem("mains frequency 60 Hz", 3));
            prop.Values.Add(new GXValueItem("GPS (global positioning system)", 4));
            prop.Values.Add(new GXValueItem("DCF 77 (Braunschweig time standard)", 5));
            prop.ForcePresetValues = true;
            prop.DefaultValue = "Not defined";            
            prop.AccessMode = Gurux.Device.AccessMode.Read;
        }

        public override void Initialize(object info)
        {
            GXObisCode code = info as GXObisCode;
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Logical Name", 1));
            GXDLMSProperty prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Time", 2);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Time Zone", 3));
            prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Status", 4);
            UpdateStatusValueItems(prop);
            this.Properties.Add(prop);
            prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Daylight Savings begin", 5);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);            
            prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Daylight Savings end", 6);
            prop.ValueType = typeof(DateTime);
            this.Properties.Add(prop);
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Deviation", 7));
            this.Properties.Add(new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Daylight Savings Enabled", 8));
            prop = new GXDLMSProperty(this.ObjectType, this.LogicalName, this.ShortName, "Clock Base", 9);
            UpdateClockBaseValueItems(prop);
            this.Properties.Add(prop);
        }
    }
}
