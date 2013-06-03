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
    [DataContract()]
    public class GXDLMSRegister : GXDLMSProperty
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public GXDLMSRegister()
        {
            this.AttributeOrdinal = 2;
        }

        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        [ReadOnly(true)]
        public override Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.Register;
            }
        }

        /// <summary>
        /// COSEM object's Short Name.
        /// </summary>
        /// <remarks>
        /// Short name is not shown on the property grid.
        /// </remarks>
        [DefaultValue(0.0), DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        public double Scaler
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
        [DefaultValue(null), DataMember(IsRequired = false, EmitDefaultValue = false)]
        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        public override string Unit
        {
            get;
            set;
        }       
    }
}
