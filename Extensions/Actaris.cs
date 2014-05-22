using System;
using System.Collections.Generic;
using System.Text;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using GXDLMS.Common;
using Gurux.Device;
using Gurux.DLMS.AddIn;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS;
using Gurux.DLMS.Objects;

namespace Extensions
{
    public class Actaris : IGXManufacturerExtension
    {
        int Unit = 0;
        int TimeOffset = 0;
        DateTime LastDateTime;

        #region IGXManufacturerExtension Members

        public object GetStart(object start)
        {
            return start;
        }

        public object GetEnd(object end)
        {
            if (end is DateTime)
            {
                if (((DateTime)end).Minute == 59)
                {
                    end = ((DateTime)end).AddMinutes(1);
                }
            }
            return end;
        }

        GXObisCode CreateItem(ObjectType objectType, int attributeIndex, string name)
        {
            GXObisCode item = new GXObisCode();
            item.ObjectType = objectType;
            item.LogicalName = name;
            item.AttributeIndex = attributeIndex;            
            return item;
        }

        public void ReadExtraInfo(List<GXObisCode> items)
        {
            //Time delay.
            items.Add(new GXObisCode("1.1.0.8.5.255", ObjectType.Register, 2));
            //Scaler and unit.
            items.Add(new GXObisCode("1.1.0.8.5.255", ObjectType.Register, 3));
        }

        public void UpdateExtraInfo(Dictionary<GXObisCode, object> items)
        {
            foreach(GXObisCode it in items.Keys)
            {
                //Time delay.
                if (it.LogicalName == "1.1.0.8.5.255" && it.AttributeIndex == 2)
                {
                    object value = items[it];
                    TimeOffset = Convert.ToInt32(value);
                }
                else if (it.LogicalName == "1.1.0.8.5.255" && it.AttributeIndex == 3)
                {
                    object value = items[it];
                    Unit = Convert.ToInt32(((object[])value)[1]);                    
                }
            }
        }

        void AddColumn(GXDLMSClient cosem, GXManufacturer man, GXDLMSTable table, byte[] ln, object scalar, int index, string name)
        {
            string logicanName = null;            
            if (ln != null)
            {
                logicanName = GXHelpers.ConvertFromDLMS(ln, DataType.OctetString, DataType.OctetString, false).ToString();
                if (string.IsNullOrEmpty(name))
                {
                    GXObisCode code = man.ObisCodes.FindByLN(ObjectType.None, logicanName, null);
                    if (code != null)
                    {
                        name = code.Description;
                    }
                    else
                    {
                        name = logicanName;
                    }
                }
            }
            if (scalar == null)
            {
                GXDLMSProperty prop = new GXDLMSProperty(ObjectType.Data, logicanName, 0, name, index, DataType.DateTime);
                prop.ValueType = DataType.DateTime;
                prop.AccessMode = Gurux.Device.AccessMode.Read;
                table.Columns.Add(prop);
            }
            else
            {
                object[] tmp = (object[])scalar;
                Gurux.DLMS.AddIn.GXDLMSRegister prop = new Gurux.DLMS.AddIn.GXDLMSRegister();
                prop.Scaler = Math.Pow(10, Convert.ToInt32(tmp[0]));
                prop.Unit = tmp[1].ToString();
                prop.LogicalName = logicanName;
                prop.Name = name;
                prop.AttributeOrdinal = index;
                prop.DLMSType = DataType.UInt32;
                prop.ValueType = DataType.Float64;
                prop.AccessMode = Gurux.Device.AccessMode.Read;
                table.Columns.Add(prop);
            }
        }

        public void UpdateColumns(GXDLMSDevice Device, 
                    GXManufacturer man, GXDLMSClient cosem,
                    Gurux.Common.IGXMedia media, int wt, Gurux.DLMS.Objects.GXDLMSObject it,
                    GXDLMSTable table, GXDLMSAddIn parent, GXCategory dataItems, GXCategory registers)
        {
            //Reading data.                
            //If Load profile1
            if (it.LogicalName == "0.0.99.1.2.255")
            {
                //Read Load Profile 1 information
                byte[] allData = parent.ReadDataBlock(cosem, media, cosem.Read("0.0.99.128.1.255", ObjectType.ProfileGeneric, 2)[0], wt, 0);
                object[] items = (object[])((object[])cosem.GetValue(allData)).GetValue(0);
                int pos = 0;
                AddColumn(cosem, man, table, null, null, 0, "DateTime");
                AddColumn(cosem, man, table, null, null, 1, "Status");
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 2, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 3, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 4, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 5, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 6, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 7, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 8, null);
                pos += 2;
                AddColumn(cosem, man, table, (byte[])items.GetValue(pos), items.GetValue(pos + 1), 9, null);                
                return;
            }           
            parent.CreateColumns(media, Device, man, wt, cosem, this, dataItems, registers, it, table);                    
        }

        public bool UpdateTableData(List<GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>> TableColumns, GXDLMSTable target, Array reply, List<object[]> rows)
        {
            List<Object> cols = new List<Object>();
            double skalar = 1;
            bool skipRow = false;
            foreach (object[] row in reply)
            {
                cols.Clear();
                for (int col = 0; col != TableColumns.Count; ++col)
                {
                    string ln = TableColumns[col].Key.LogicalName;
                    if (ln == "0.0.96.55.1.255")   //LoadProfile1EndOfIntervalDate
                    {
                        if (Unit == 6) //Minutes
                        {
                            skalar = TimeOffset;
                            skalar /= 60;
                        }
                        object tmStatus = ((object[])row.GetValue(col));
                        object tm;
                        if (tmStatus != null)
                        {
                            tm = ((object[])tmStatus).GetValue(0);
                            tm = GXHelpers.ConvertFromDLMS(tm, DataType.OctetString, DataType.DateTime, false);
                            LastDateTime = (DateTime)tm;
                        }
                        LastDateTime = LastDateTime.AddMinutes(TimeOffset);
                        cols.Add(LastDateTime);
                    }
                    else if (ln == "0.0.96.55.2.255")   //LoadProfile1EndOfIntervalDate
                    {
                        object tmStatus = ((object[])row.GetValue(col));
                        object tm;
                        if (tmStatus != null)
                        {
                            tm = ((object[])tmStatus).GetValue(0);
                            tm = GXHelpers.ConvertFromDLMS(tm, DataType.OctetString, DataType.DateTime, false);
                            cols.Add((DateTime)tm);
                        }
                        else
                        {
                            cols.Add(LastDateTime.AddMinutes(TimeOffset));
                        }
                    }
                    else if (ln == "0.0.96.55.1.255" || //LoadProfile1StartOfIntervalDate
                             ln == "0.0.96.56.1.255")   //LoadProfile2StartOfIntervalDate                                
                    {
                        object tmStatus = row.GetValue(col);
                        object tm = null;
                        if (tmStatus != null)
                        {
                            tm = ((object[])tmStatus).GetValue(0);
                            tm = GXHelpers.ConvertFromDLMS(tm, DataType.OctetString, DataType.DateTime, false);
                        }
                        cols.Add(tm);
                    }
                    else if (ln == "0.0.96.55.7.255" ||//LoadProfile1EndOfRecordingData
                            ln == "0.0.96.56.7.255")//LoadProfile2EndOfRecordingData
                    {
                        if (Unit == 6) //Minutes
                        {
                            skalar = TimeOffset;
                            skalar /= 60;
                        }                         
                        object val = ((object[])row.GetValue(col));
                        //Start of interval and linked status, or NULL data if not significative
                        object tmStatus = ((object[])((object[])val).GetValue(0));
                        object status = 0;
                        if (tmStatus != null)
                        {
                            object tm = ((object[])tmStatus).GetValue(0);
                            tm = GXHelpers.ConvertFromDLMS(tm, DataType.OctetString, DataType.DateTime, false);
                            if ((DateTime)tm == DateTime.MinValue)
                            {
                                skipRow = true;
                                break;
                            }
                            LastDateTime = (DateTime)tm;
                            System.Diagnostics.Debug.WriteLine("New data started: " + LastDateTime.ToString());
                            status = ((object[])tmStatus).GetValue(1);
                        }
                        LastDateTime = LastDateTime.AddMinutes(TimeOffset);
                        cols.Add(LastDateTime);
                        cols.Add(status);
                        // End date and linked status, or NULL data if not significative
                        // Time 1 and linked status, or NULL data if not significative
                        //Time 2 and linked status, or NULL data if not significative
                        //Add channel 1
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(4)) * skalar);
                        //Add channel 2
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(5)) * skalar);
                        //Add channel 3
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(6)) * skalar);
                        //Add channel 4
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(7)) * skalar);
                        //Add channel 5
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(8)) * skalar);
                        //Add channel 6
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(9)) * skalar);
                        //Add channel 7
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(10)) * skalar);
                        //Add channel 8
                        cols.Add(Convert.ToDouble(((object[])val).GetValue(11)) * skalar);
                    }
                    else
                    {
                        if (skalar == 1)
                        {
                            cols.Add(row.GetValue(col));
                        }
                        else
                        {
                            cols.Add(Convert.ToDouble(row.GetValue(col)) * skalar);
                        }
                        //cols.AddRange(row);
                    }
                }
                if (!skipRow)
                {
                    rows.Add(cols.ToArray());
                }
            }
            return true;
        }
        
        #endregion
    }
}
