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
    public class GXDLMSData : GXDLMSProperty
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GXDLMSData()
        {
            this.AttributeOrdinal = 2;
        }

        [ValueAccess(ValueAccessType.Show, ValueAccessType.None)]
        [GXUserLevelAttribute(UserLevelType.Experienced)]
        public override Gurux.DLMS.ObjectType ObjectType
        {
            get
            {
                return Gurux.DLMS.ObjectType.Data;
            }            
        }
    }
}
