using System;
using System.Collections.Generic;
using Gurux.DLMS;
using Gurux.Device;
using Gurux.DLMS.AddIn;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;

namespace Gurux.DLMS.AddIn.ManufacturerSettings
{
    public enum ProcessType
    {
        Read,
        Write        
    }

    /// <summary>
    /// With this interface it is possible to implement manufacturer spesific transactions.
    /// </summary>
    public interface IGXManufacturerExtension
    {
        /// <summary>
        /// Get manufacturer spesific start time.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        object GetStart(object start);
        
        /// <summary>
        /// Get manufacturer spesific end time.
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        object GetEnd(object end);

        /// <summary>
        /// Read extra info from meter.
        /// </summary>
        /// <param name="items"></param>
        void ReadExtraInfo(List<GXObisCode> items);
        /// <summary>
        /// Update read extra info.
        /// </summary>
        /// <param name="items"></param>
        void UpdateExtraInfo(Dictionary<GXObisCode, object> items);

        void UpdateColumns(Gurux.DLMS.AddIn.GXDLMSDevice Device, GXManufacturer man, Gurux.DLMS.GXDLMSClient cosem, Gurux.Common.IGXMedia media, int wt, GXDLMSObject item, Gurux.DLMS.AddIn.GXDLMSTable table, Gurux.DLMS.AddIn.GXDLMSAddIn parent, GXCategory dataItems, GXCategory registers);

        bool UpdateTableData(List<GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>> TableColumns, GXDLMSTable target, Array reply, List<object[]> rows);
   }
}
