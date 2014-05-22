using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurux.Device;
using System.Runtime.Serialization;
using Gurux.Device.Editor;
using System.ComponentModel;

namespace Gurux.DLMS.AddIn
{
    /// <summary>
    /// Cosem profile generic data.
    /// </summary>
    [GXReadMessage("ReadTableInfo", "UpdateTableInfo", "IsAllTableInfoReceived", "ReadNextInfo", Index = 1)]
    //Read table columns.
    [GXReadMessage("ReadTableContent", "UpdateTableContent", "IsAllTableDataReceived", "ReadNext", Index = 2)]
    //Read capture period of the table.
    [GXReadMessage("ReadCapturePeriod", "UpdateCapturePeriod", Index = 3)]    
    //Read table data.
    [GXReadMessage("ReadTableData", "UpdateTableData", "IsAllTableDataReceived", "ReadNext", Index = 4)]
    [DataContract()]
	[TypeConverter(typeof(GXDLMSObjectTypeConverter))]
    public class GXDLMSTable : GXTable, IGXPartialRead
    {
        public GXDLMSTable()
        {
            //In default read new values.
            ((IGXPartialRead)this).Type = PartialReadType.New;
        }        


        /// <summary>
        /// COSEM object's Short Name.
        /// </summary>
        /// <remarks>
        /// Short name is not shown on the property grid.
        /// </remarks>
        [DataMember(IsRequired = false, EmitDefaultValue = false), DefaultValue(0)]
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public int ShortName
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
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public string LogicalName
        {
            get;
            set;
        }

        /// <summary>
        /// How often values are captured.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.Show)]
        public int CapturePeriod
        {
            get;
            set;
        }

        /// <summary>
        /// How columns are sorted.
        /// </summary>       
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.Show)]
        [DefaultValue(Gurux.DLMS.Objects.SortMethod.FiFo)]
        public Gurux.DLMS.Objects.SortMethod SortMethod
        {
            get;
            set;
        }


        /// <summary>
        /// Entries (rows) in Use.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.Show)]
        public int EntriesInUse
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum Entries (rows) count.
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ReadOnly(false), ValueAccess(ValueAccessType.Show, ValueAccessType.Show)]
        public int ProfileEntries
        {
            get;
            set;
        }

        public override void Validate(bool designMode, GXTaskCollection tasks)
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
            base.Validate(designMode, tasks);			
		}

        #region IGXPartialRead Members
        [DefaultValue(PartialReadType.New)]
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.Edit)]
        public PartialReadType Type
        {
            get;
            set;
        }

        [DefaultValue(null), DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.None, ValueAccessType.Edit)]
        public object Start
        {
            get;
            set;
        }

        [ValueAccess(ValueAccessType.None, ValueAccessType.Edit)]
        [DefaultValue(null), DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object End
        {
            get;
            set;
        }       

        #endregion
    }
}
