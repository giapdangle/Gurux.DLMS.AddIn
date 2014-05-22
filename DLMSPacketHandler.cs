using System;
using System.Collections.Generic;
using System.Text;
using Gurux.Common;
using Gurux.Communication;
using Gurux.Net;
using Gurux.Serial;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using System.Linq;
using Gurux.Device.Editor;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using Gurux.DLMS.Internal;

namespace Gurux.DLMS.AddIn
{
    public class DLMSPacketHandler : Gurux.Device.IGXPacketHandler
    {
        Gurux.DLMS.Objects.GXDLMSProfileGeneric pg = null;
		Dictionary<GXObisCode, object> ExtraInfo = new Dictionary<GXObisCode, object>();
        int ExtraInfoPos = 0;
        List<GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>> TableColumns = null;
        IGXManufacturerExtension Extension;
        Gurux.DLMS.DataType ObjectDataType;
        Array AARQRequest;
        GXDLMSClient parser = null;
        int ReceivedRows = 0, AARQRequestPos;
        /// <summary>
        /// All transactions are not notified because it's slow with DLMS. Only full percents are notified.
        /// </summary>
        int LastNotifiedTransactionProgress = 0;        
        byte[] ReceivedData;
        RequestTypes IsMoreDataAvailable = RequestTypes.None;
        bool TryParse = false, SupportNetworkSpecificSettings;
        int[] ColumnIndexs = null;

        static public string GetArrayAsString(object data)
        {
            Array arr = (Array)data;
            string str = null;
            foreach (object it in arr)
            {
                if (str == null)
                {
                    str = "{";
                }
                else
                {
                    str += ", ";
                }
                if (it != null && it.GetType().IsArray)
                {
                    str += GetArrayAsString(it);
                }
                else
                {
                    str += Convert.ToString(it);
                }
            }
            str += "}";
            return str;
        }

        #region IGXPacketHandler Members

        public object Parent
        {
            get;
            set;
        }

        public void Connect(object sender)
        {
        }

        public void Disconnect(object sender)
        {

        }
        
        public object DeviceValueToUIValue(Gurux.Device.GXProperty sender, object value)
        {
            return value;
        }

        void InitRead(object sender, GXPacket GXPacket)
        {
            LastNotifiedTransactionProgress = 0;
            parser = new Gurux.DLMS.GXDLMSClient();
            GXDLMSDevice device = sender as GXDLMSDevice;
            parser.UseLogicalNameReferencing = device.UseLogicalNameReferencing;
            Gurux.Common.IGXMedia media = device.GXClient.Media as Gurux.Common.IGXMedia;
            SupportNetworkSpecificSettings = device.SupportNetworkSpecificSettings && media is GXNet;            
            if (SupportNetworkSpecificSettings)
            {
                parser.InterfaceType = Gurux.DLMS.InterfaceType.Net;
            }
            else
            {
                media.Eop = (byte)0x7E;
            }
            if (device.Manufacturers == null)
            {
                device.Manufacturers = new GXManufacturerCollection();
                GXManufacturerCollection.ReadManufacturerSettings(device.Manufacturers);
            }
            GXManufacturer man = device.Manufacturers.FindByIdentification(device.Identification);
			if (man == null)
			{
				throw new Exception("Unknown DLMS manufacturer type: " + device.Identification);
			}
            if (!string.IsNullOrEmpty(man.Extension))
            {
                Type t = Type.GetType(man.Extension);
                Extension = Activator.CreateInstance(t) as IGXManufacturerExtension;
            }            
            if (media is GXSerial && device.StartProtocol == StartProtocolType.IEC)
            {                                
                byte Terminator = 0xA;
                GXSerial serial = media as GXSerial;
                serial.Eop = Terminator;
                ReceiveParameters<string> p = new ReceiveParameters<string>()
                {
                    Eop = Terminator,                    
                    WaitTime = device.WaitTime * 1000                    
                };
                lock (media.Synchronous)
                {
                    //Init IEC connection. This must done first with serial connections.
                    string data = "/?!\r\n";
                    if (device.HDLCAddressing == HDLCAddressType.SerialNumber)
                    {
                        data = "/?" + device.SerialNumber + "!\r\n";
                    }
                    media.Send(data, null);
                    if (!media.Receive(p))
                    {
                        //Try to move away from mode E.
                        //TODO: this.ReadDLMSPacket(this.DisconnectRequest());
                        data = "Failed to receive reply from the device in given time.";
                        GXLogWriter.WriteLog(data);
                        throw new Exception(data);
                    }
                    //If echo is used.
                    if (p.Reply == data)
                    {
                        p.Reply = null;
                        if (!media.Receive(p))
                        {
                            //Try to move away from mode E.
                            //TODO: this.ReadDLMSPacket(this.DisconnectRequest());
                            data = "Failed to receive reply from the device in given time.";
                            GXLogWriter.WriteLog(data);
                            throw new Exception(data);
                        }
                    }
                }    
                string manufactureID = p.Reply.Substring(1, 3);
                char baudrate = p.Reply[4];
                int bitrate = 0;
                switch (baudrate)
                {
                    case '0':
                        bitrate = 300;
                        break;
                    case '1':
                        bitrate = 600;
                        break;
                    case '2':
                        bitrate = 1200;
                        break;
                    case '3':
                        bitrate = 2400;
                        break;
                    case '4':
                        bitrate = 4800;
                        break;
                    case '5':
                        bitrate = 9600;
                        break;
                    case '6':
                        bitrate = 19200;
                        break;
                    default:
                        throw new Exception("Unknown baud rate.");
                }                
                //Send ACK
                //Send Protocol control character
                byte controlCharacter = (byte)'2';// "2" HDLC protocol procedure (Mode E)
                //Send Baudrate character
                //Mode control character 
                byte ModeControlCharacter = (byte)'2';//"2" //(HDLC protocol procedure) (Binary mode)
                //We are not receive anything.
                lock (media.Synchronous)
                {
                    media.Send(new byte[] { 0x06, controlCharacter, (byte)baudrate, ModeControlCharacter, 0x8D, 0x0A }, null);
                    System.Threading.Thread.Sleep(1000);
                    serial.BaudRate = bitrate;
                    ReceiveParameters<byte[]> args = new ReceiveParameters<byte[]>();
                    args.Eop = (byte)0x0A;
                    args.WaitTime = 500;
                    media.Receive(args);
                }
                serial.DataBits = 8;
                serial.Parity = System.IO.Ports.Parity.None;
                serial.StopBits = System.IO.Ports.StopBits.One;
                serial.ResetSynchronousBuffer();
                serial.Eop = (byte)0x7E;
            }            
        }

        //Set the secondary station in connected mode and reset its sequence number variables.
        void ReadSNRMSend(object sender, GXPacket GXPacket)
        {
            GXDLMSDevice Device = sender as GXDLMSDevice;
            AARQRequestPos = 0;
            //Is connection made using network.
            bool NetConnect = !Device.UseRemoteSerial && Device.SupportNetworkSpecificSettings && Device.GXClient.Media is GXNet;
            //TODO: GXProperty.Device.KeepAliveEnabled = false;
            parser.Authentication = Device.Authentication;
            if (!string.IsNullOrEmpty(Device.Password))
            {
                parser.Password = ASCIIEncoding.ASCII.GetBytes(Device.Password);
            }
            else
            {
                parser.Password = null;
            }
            //GXCom is counting BOP, EOP and CRC.
            parser.GenerateFrame = false;
            //If Serial Number is used.	
            HDLCAddressType addressing = Device.HDLCAddressing;
             //If network media is used check is manufacturer supporting IEC 62056-47
            if (NetConnect)
            {
                GXPacket.Bop = GXPacket.Eop = null;
                GXPacket.ChecksumSettings.Type = ChecksumType.None;
                parser.InterfaceType = Gurux.DLMS.InterfaceType.Net;
                parser.ClientID = Convert.ToUInt16(Device.ClientID);
                parser.ServerID = Convert.ToUInt16(Device.PhysicalAddress);
            }
            else
            {
                if (addressing == HDLCAddressType.Custom)
                {
                    parser.ClientID = Device.ClientID;
                }
                else
                {
                    parser.ClientID = (byte)(Convert.ToByte(Device.ClientID) << 1 | 0x1);
                }
                if (addressing == HDLCAddressType.SerialNumber)
                {
                    parser.ServerID = GXManufacturer.CountServerAddress(addressing, Device.SNFormula, Convert.ToUInt32(Device.SerialNumber), Device.LogicalAddress);
                }
                else
                {
                    parser.ServerID = GXManufacturer.CountServerAddress(addressing, Device.SNFormula, Device.PhysicalAddress, Device.LogicalAddress);
                }
                object clientid = 0;
                if (parser.Authentication == Gurux.DLMS.Authentication.None)
                {
                    clientid = Device.ClientID;
                }
                else if (parser.Authentication == Gurux.DLMS.Authentication.Low)
                {
                    clientid = Device.ClientIDLow;
                }
                else if (parser.Authentication == Gurux.DLMS.Authentication.High)
                {
                    clientid = Device.ClientIDHigh;
                }
                if (addressing == HDLCAddressType.Custom)
                {
                    parser.ClientID = clientid;
                }
                else
                {
                    parser.ClientID = (byte)(Convert.ToByte(clientid) << 1 | 0x1);
                }
            }
            // If Network standard is used SNRM message is not send.
            if (SupportNetworkSpecificSettings)
            {
                GXPacket.ClearData();
            }
            else
            {
                GXPacket.AppendData(parser.SNRMRequest());
            }
        }

        // Confirm that the secondary received and acted on an SNRM or DISC command.
        void UAReply(object sender, GXPacket GXPacket)
        {                        
            byte[] data = GXPacket.ExtractPacket();
            parser.ParseUAResponse(data);                                    
        }

        void AAREReply(object sender, GXPacket packet)
        {
            GXDLMSDevice device = sender as GXDLMSDevice;
            byte[] data = packet.ExtractPacket();
            parser.ParseAAREResponse(data);            
            //parser.Objects.Add
            // Show limits as parameters.    
            /*
            if (parser.UseLogicalNameReferencing)
            {
                UpdateParameter(device.Parameters, "Attribute0SetReferencing", parser.LNSettings.Attribute0SetReferencing);
                UpdateParameter(device.Parameters, "PriorityManagement", parser.LNSettings.PriorityManagement);
                UpdateParameter(device.Parameters, "Attribute0GetReferencing", parser.LNSettings.Attribute0GetReferencing);
                UpdateParameter(device.Parameters, "GetBlockTransfer", parser.LNSettings.GetBlockTransfer);
                UpdateParameter(device.Parameters, "SetBlockTransfer", parser.LNSettings.SetBlockTransfer);
                UpdateParameter(device.Parameters, "ActionBlockTransfer", parser.LNSettings.ActionBlockTransfer);
                UpdateParameter(device.Parameters, "MultibleReferences", parser.LNSettings.MultibleReferences);
                UpdateParameter(device.Parameters, "Get", parser.LNSettings.Get);
                UpdateParameter(device.Parameters, "Set", parser.LNSettings.Set);
                UpdateParameter(device.Parameters, "Action", parser.LNSettings.Action);
                UpdateParameter(device.Parameters, "EventNotification", parser.LNSettings.EventNotification);
                UpdateParameter(device.Parameters, "SelectiveAccess", parser.LNSettings.SelectiveAccess);
            }
            else
            {
                UpdateParameter(device.Parameters, "SNReadSupported", parser.SNSettings.Read);
                UpdateParameter(device.Parameters, "SNWriteSupported", parser.SNSettings.Write);
                UpdateParameter(device.Parameters, "SNUnConfirmedWriteSupported", parser.SNSettings.UnconfirmedWrite);
                UpdateParameter(device.Parameters, "SNInformationReportSupported", parser.SNSettings.InformationReport);
                UpdateParameter(device.Parameters, "MultibleReferencesSupported", parser.SNSettings.MultibleReferences);
                UpdateParameter(device.Parameters, "ParametrizedAccessSupported", parser.SNSettings.ParametrizedAccess);
                UpdateParameter(device.Parameters, "ServerMaxPDUsize", parser.MaxReceivePDUSize);
                UpdateParameter(device.Parameters, "ValueOfQualityOfService", parser.ValueOfQualityOfService);
                UpdateParameter(device.Parameters, "ServerDLMSVersionNumber", parser.DLMSVersion);
                UpdateParameter(device.Parameters, "NumberOfUnusedBits", parser.NumberOfUnusedBits);
            }     
             * */
        }
        

        void ReadAARQ(object sender, GXPacket GXPacket)
        {            
            AARQRequest = parser.AARQRequest(null);
            int cnt = AARQRequest.Length;
            GXPacket.AppendData(AARQRequest.GetValue(AARQRequestPos));
            ++AARQRequestPos;
        }

        void ReadAARQNext(object sender, GXPacket GXPacket)
        {
            GXPacket.AppendData(AARQRequest.GetValue(AARQRequestPos));
            ++AARQRequestPos;
        }

        /// <summary>
        /// Generate read message for selected property.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="pack"></param>
        void ReadData(Gurux.Device.GXProperty property, GXPacket pack)
        {
            GXDLMSProperty prop = property as GXDLMSProperty;
            int AttributeOrdinal = ((GXDLMSProperty) prop).AttributeOrdinal;
            if (AttributeOrdinal < 1)
            {
                throw new Exception("Attribute Ordinal is invalid.");
            }
            //Read component.
            if (parser.UseLogicalNameReferencing)
            {
                pack.AppendData(parser.Read(prop.LogicalName, prop.ObjectType, AttributeOrdinal)[0]);
            }
            else //Use Short Name
            {
                uint sn = prop.ShortName;
                //If Short name not found.
                if (sn == 0)
                {
                    throw new Exception("Short name not found.");
                }
                pack.AppendData(parser.Read(sn, 0, AttributeOrdinal)[0]);
            }
        }

        void ReadDataType(Gurux.Device.GXProperty property, GXPacket pack)
        {
            GXDLMSProperty prop = property as GXDLMSProperty;
            int AttributeOrdinal = ((GXDLMSProperty)prop).AttributeOrdinal;
            if (AttributeOrdinal < 1)
            {
                throw new Exception("Attribute Ordinal is invalid.");
            }
            //Read component.
            if (parser.UseLogicalNameReferencing)
            {
                foreach (object it in parser.Read(prop.LogicalName, prop.ObjectType, AttributeOrdinal))
                {
                    pack.AppendData(it);
                }
            }
            else //Use Short Name
            {
                uint sn = prop.ShortName;
                //If Short name not found.
                if (sn == 0)
                {
                    throw new Exception("Short name not found.");
                }
                foreach (object it in parser.Read(sn, 0, AttributeOrdinal))
                {
                    pack.AppendData(it);
                }
            }
        }


        void WriteData(Gurux.Device.GXProperty property, GXPacket pack)
        {
            GXDLMSProperty prop = property as GXDLMSProperty;
            int AttributeOrdinal = ((GXDLMSProperty)prop).AttributeOrdinal;
            if (AttributeOrdinal < 1)
            {
                throw new Exception("Attribute Ordinal is invalid.");
            }
            object data = prop.GetValue(false);
            //Write component.
            if (parser.UseLogicalNameReferencing)
            {
                foreach (object it in parser.Write(prop.LogicalName, data, ObjectDataType, prop.ObjectType, AttributeOrdinal))
                {
                    pack.AppendData(it);
                }
            }
            else //Use Short Name
            {
                uint sn = prop.ShortName;
                //If Short name not found.
                if (sn == 0)
                {
                    throw new Exception("Short name not found.");
                }
                foreach (object it in parser.Write(sn, data, ObjectDataType, 0, AttributeOrdinal))
                {
                    pack.AppendData(it);
                }
            }
        }

        public void ExecuteSendCommand(object sender, string command, GXPacket packet)
        {
            if (command != "ReadAARQNext" && command != "ReadNextInfo" && command != "ReadNext")
            {
                IsMoreDataAvailable = RequestTypes.None;
            }
            GXDLMSProperty prop = sender as GXDLMSProperty;
            if (command == "InitRead")
            {
                LastNotifiedTransactionProgress = 0;
                InitRead(sender, packet);                
            }
            else if (command == "ReadSNRMSend")
            {
                LastNotifiedTransactionProgress = 0;
                ReadSNRMSend(sender, packet);            
            }
            else if (command == "ReadAARQ")
            {
                LastNotifiedTransactionProgress = 0;
                ReadAARQ(sender, packet);
            }
            else if (command == "ReadAARQNext")
            {
                LastNotifiedTransactionProgress = 0;
                ReadAARQNext(sender, packet);
            }            
            else if (command == "ReadData")
            {
                LastNotifiedTransactionProgress = 0;
                ReadData(prop, packet);
            }
            else if (command == "ReadDataType")
            {
                LastNotifiedTransactionProgress = 0;
                ReadDataType(prop, packet);                
            }
            else if (command == "WriteData")
            {
                LastNotifiedTransactionProgress = 0;
                WriteData(prop, packet);                
            }
            else if (command == "KeepAliveRequest")
            {
                LastNotifiedTransactionProgress = 0;
                //Network connection don't need this.
                if (!SupportNetworkSpecificSettings)
                {
                    packet.AppendData(parser.GetKeepAlive());
                }
            }
            else if (command == "DisconnectRequest")
            {
                LastNotifiedTransactionProgress = 0;
                //Network connection don't need this.
                if (!SupportNetworkSpecificSettings)
                {
                    packet.AppendData(parser.DisconnectRequest());
                }
            }
            else if (command == "ReadTableInfo")
            {
                if (this.Extension != null)
                {
                    ReceivedData = null;
                    ExtraInfoPos = 0;
                    ExtraInfo.Clear();
                    List<GXObisCode> items = new List<GXObisCode>();
                    this.Extension.ReadExtraInfo(items);
                    if (items.Count != 0)
                    {
                        foreach (GXObisCode it in items)
                        {
                            ExtraInfo.Add(it, null);
                        }
                        GXObisCode item = items[0];
                        packet.AppendData(parser.Read(item.LogicalName, item.ObjectType, item.AttributeIndex)[0]);
                    }
                }
            }
            else if (command == "ReadCapturePeriod")
            {
                ReceivedData = null;                
                GXDLMSTable table = sender as GXDLMSTable;
                //Capture period is read only once.
                if (table.CapturePeriod == 0)
                {
                    if (parser.UseLogicalNameReferencing)
                    {
                        packet.AppendData(parser.Read(table.LogicalName, Gurux.DLMS.ObjectType.ProfileGeneric, 4)[0]);
                    }
                    else
                    {
                        packet.AppendData(parser.Read(table.ShortName, Gurux.DLMS.ObjectType.ProfileGeneric, 4)[0]);
                    }
                }
            }                
            else if (command == "ReadTableContent")
            {
                //Check start and end times.
                DateTime starttm, endtm;
                GXDLMSTable table = sender as GXDLMSTable;
                (table as IGXPartialRead).GetStartEndTime(out starttm, out endtm);
                LastNotifiedTransactionProgress = 0;                
                ReceivedData = null;
                TryParse = false;
                if (parser.UseLogicalNameReferencing)
                {
                    packet.AppendData(parser.Read(table.LogicalName, Gurux.DLMS.ObjectType.ProfileGeneric, 3)[0]);
                }
                else
                {
                    packet.AppendData(parser.Read(table.ShortName, Gurux.DLMS.ObjectType.ProfileGeneric, 3)[0]);
                }
            }
            else if (command == "ReadNextInfo") //Read next part of the data or next item.
            {
                //Read next extra info.
                if (IsMoreDataAvailable == Gurux.DLMS.RequestTypes.None)
                {                                        
                    GXObisCode item = ExtraInfo.Keys.ElementAt(ExtraInfoPos);
                    object data = parser.Read(item.LogicalName, item.ObjectType, item.AttributeIndex)[0];
                    packet.AppendData(data);
                }
                else //Read next part of the data.
                {
                    Gurux.DLMS.RequestTypes readItem = Gurux.DLMS.RequestTypes.Frame;
                    if ((IsMoreDataAvailable & RequestTypes.Frame) == 0)
                    {
                        readItem = Gurux.DLMS.RequestTypes.DataBlock;
                    }
                    packet.AppendData(parser.ReceiverReady(readItem));
                }
            }
            else if (command == "ReadNext") //Read next part of data.
            {
                Gurux.DLMS.RequestTypes readItem = Gurux.DLMS.RequestTypes.Frame;
                if ((IsMoreDataAvailable & RequestTypes.Frame) == 0)
                {
                    readItem = Gurux.DLMS.RequestTypes.DataBlock;
                }
                packet.AppendData(parser.ReceiverReady(readItem));
            }
            else if (command == "ReadTableData") //Read next part of data.
            {
                pg = Gurux.DLMS.GXDLMSClient.CreateObject(ObjectType.ProfileGeneric) as Gurux.DLMS.Objects.GXDLMSProfileGeneric;
                GXDLMSTable table2 = sender as GXDLMSTable;
                pg.CapturePeriod = table2.CapturePeriod;
                foreach (GXDLMSProperty col in table2.Columns)
                {
                    GXDLMSObject item = Gurux.DLMS.GXDLMSClient.CreateObject(col.ObjectType);
                    item.SetDataType(col.AttributeOrdinal, col.DLMSType);
                    item.SetUIDataType(col.AttributeOrdinal, col.ValueType);
                    item.LogicalName = col.LogicalName;
                    //Update scaler for register.
                    if (col is GXDLMSRegister && col.AttributeOrdinal == 2)
                    {
                        GXDLMSRegister r = col as GXDLMSRegister;
                        if (item.ObjectType == ObjectType.DemandRegister)
                        {
                            (item as Gurux.DLMS.Objects.GXDLMSDemandRegister).Scaler = r.Scaler;
                        }
                        else
                        {
                            (item as Gurux.DLMS.Objects.GXDLMSRegister).Scaler = r.Scaler;
                        }
                    }
                    pg.CaptureObjects.Add(new GXKeyValuePair<GXDLMSObject, GXDLMSCaptureObject>(item, new GXDLMSCaptureObject(col.AttributeOrdinal, 0)));
                }
                LastNotifiedTransactionProgress = 0;
                GXDLMSTable table = sender as GXDLMSTable;                
                ReceivedRows = 0;
                TryParse = true;
                ReceivedData = null;                
                IGXPartialRead partialRead = table as IGXPartialRead;
                object name, data;
                int EDISType = 0, Version = 0;
                if (parser.UseLogicalNameReferencing)
                {
                    name = table.LogicalName;
                }
                else
                {
                    name = table.ShortName;
                }
                string ClockLN = "0.0.1.0.0.255";
                if (partialRead.Type == PartialReadType.All)
                {
                    data = parser.Read(name, ObjectType.ProfileGeneric, 2)[0];
                }
                else if (partialRead.Type == PartialReadType.Entry)
                {
                    data = parser.ReadRowsByEntry(name, Convert.ToInt32((table as IGXPartialRead).Start), Convert.ToInt32((table as IGXPartialRead).End));
                }                
                else
                {                    
                    DateTime starttm = DateTime.MinValue, endtm = DateTime.MaxValue;                                        
                    if (table.Columns.Count != 0)
                    {
                        GXDLMSProperty col = table.Columns[0] as GXDLMSProperty;
                        EDISType = col.AttributeOrdinal;
                        Version = col.Version;
                    }
                    (table as IGXPartialRead).GetStartEndTime(out starttm, out endtm);
                    System.Diagnostics.Debug.WriteLine("Reading table between: " + starttm.ToString() + "" + endtm.ToString());
                    data = parser.ReadRowsByRange(name, ClockLN, Gurux.DLMS.ObjectType.Clock, Version, starttm, endtm);                    
                }
                packet.AppendData(data);
            }
            else
            {
                throw new NotImplementedException();
            }
        }      
              
        int CheckErrors(byte[] reply, bool throwException)
        {
            object[,] errors = parser.CheckReplyErrors(null, reply);
            if (errors != null)
            {
                if (throwException || (int)errors[0, 0] != 3)
                {
                    int error = (int)errors[0, 0];
                    throw new GXDLMSException(error);
                }
                else
                {
                    return (int)errors[0, 0];
                }
            }            
            return 0;
        }

        public void ExecuteParseCommand(object sender, string command, GXPacket[] packets)
        {            
            GXPacket[] arr = packets;            
            if (command == "UAReply")
            {
                UAReply(sender, arr[0]);
            }
            else if (command == "AAREReply")
            {
                AAREReply(sender, arr[0]);
            }
            else if (command == "CheckReplyPacket")
            {
                //Get data as byte array.
                byte[] reply = arr[0].ExtractPacket();
                CheckErrors(reply, true);
            }
            else if (command == "ReadDataReply") //Update property value
            {                
                byte[] data = null;
                //Get data as byte array.
                foreach (GXPacket it in arr)
                {
                    byte[] reply = it.ExtractPacket();
                    CheckErrors(reply, false);
                    parser.GetDataFromPacket(reply, ref data);
                }
                GXDLMSProperty prop = sender as GXDLMSProperty;
                object value;
                if (prop.ObjectType == ObjectType.Data || 
                    prop.ObjectType == ObjectType.Register ||
                    prop.ObjectType == ObjectType.ExtendedRegister ||
                    prop.ObjectType == ObjectType.DemandRegister)
                {
                    GXDLMSObject obj = Gurux.DLMS.GXDLMSClient.CreateObject(prop.ObjectType);
                    if (prop.DLMSType == DataType.None)
                    {
                        prop.DLMSType = parser.GetDLMSDataType(data);
                    }
                    obj.SetDataType(prop.AttributeOrdinal, prop.DLMSType);                    
                    obj.SetUIDataType(prop.AttributeOrdinal, prop.ValueType);
                    obj.LogicalName = prop.LogicalName;
                    //Update scaler if register.
                    if (prop is GXDLMSRegister)
                    {
                        GXDLMSRegister r = prop as GXDLMSRegister;
                        if (obj is GXDLMSDemandRegister)
                        {
                            (obj as GXDLMSDemandRegister).Scaler = r.Scaler;
                        }
                        else
                        {
                            (obj as Gurux.DLMS.Objects.GXDLMSRegister).Scaler = r.Scaler;
                        }
                    }

                    parser.UpdateValue(data, obj, prop.AttributeOrdinal);
                    value = obj.GetValues()[prop.AttributeOrdinal - 1];
                }
                else if (prop.Parent.Parent is GXDLMSCategory)
                {
                    GXDLMSCategory cat = prop.Parent.Parent as GXDLMSCategory;
                    GXDLMSObject obj = Gurux.DLMS.GXDLMSClient.CreateObject(cat.ObjectType);
                    obj.LogicalName = prop.LogicalName;
                    parser.UpdateValue(data, obj, prop.AttributeOrdinal);
                    value = obj.GetValues()[prop.AttributeOrdinal - 1];
                }
                else
                {
                    throw new Exception("Invalid target.");
                }
                prop.ReadTime = DateTime.Now;
                prop.SetValue(value, false, Gurux.Device.PropertyStates.ValueChangedByDevice);
            }
            else if (command == "ReadDataTypeReply") //Update property value
            {
                byte[] data = null;
                //Get data as byte array.
                foreach (GXPacket it in arr)
                {
                    byte[] reply = it.ExtractPacket();
                    CheckErrors(reply, false);
                    parser.GetDataFromPacket(reply, ref data);
                }                
                ObjectDataType = parser.GetDLMSDataType(data);
            }
            else if (command == "WriteDataReply")
            {
                //Get data as byte array.
                foreach (GXPacket it in arr)
                {
                    byte[] reply = it.ExtractPacket();
                    CheckErrors(reply, false);
                }
                Gurux.Device.GXProperty prop = sender as Gurux.Device.GXProperty;
                prop.WriteTime = DateTime.Now;
                prop.NotifyPropertyChange(Gurux.Device.PropertyStates.ValueChangedByDevice);
            }
            else if (command == "UpdateTableContent")//Get table columns after all all data is received.
            {                
                TableColumns = parser.ParseColumns(ReceivedData);
                GXDLMSTable table = sender as GXDLMSTable;
                List<int> values = new List<int>();
                int pos = 0;
                foreach (var it in TableColumns)
                {
                    foreach (GXDLMSProperty prop in table.Columns)
                    {
                        if (string.Compare(prop.LogicalName, it.Key.LogicalName, true) == 0)
                        {
                            values.Add(pos);
                            break;
                        }
                    }
                    ++pos;
                }
                ColumnIndexs = values.ToArray();
            }
            else if (command == "UpdateTableInfo")//Update read extra info
            {
                if (this.Extension != null)
                {
                    Dictionary<GXObisCode, object> items = new Dictionary<GXObisCode, object>(ExtraInfo);
                    this.Extension.UpdateExtraInfo(items);                    
                }
            }
            else if (command == "UpdateCapturePeriod")//Update Capture Period of the table.
            {
                GXDLMSTable table = sender as GXDLMSTable;
                //Get data as byte array.
                foreach (GXPacket it in arr)
                {
                    byte[] allData = null;
                    byte[] reply = it.ExtractPacket();
                    CheckErrors(reply, false);
                    parser.GetDataFromPacket(reply, ref allData);
                    table.CapturePeriod = Convert.ToInt32(parser.GetValue(allData));
                }                                
            }
                
            else if (command == "UpdateTableData")//Get table columns after all all data is received.
            {
                pg = null;
                if (!this.TryParse)
                {
                    //Parse DLMS objexts.                    
                    Array reply = (Array)parser.GetValue(ReceivedData);
                    GXDLMSTable table = sender as GXDLMSTable;
                    // If there is no data.
                    if (reply.Length == 0)
                    {
                        table.ClearRows();
                    }
                    else
                    {
                        int count = table.RowCount;
                        List<object[]> rows = new List<object[]>(reply.Length);
                        foreach (object row in reply)
                        {
                            rows.Add((object[])row);
                        }
                        table.AddRows(count, rows, false);
                        //Get rows back because DeviceValueToUiValue is called.
                        rows = table.GetRows(count, reply.Length, true);
                        Gurux.Device.Editor.IGXPartialRead partialRead = table as Gurux.Device.Editor.IGXPartialRead;
                        //Save latest read time. Note we must add one second or we will read last values again.
                        if (partialRead.Type == Gurux.Device.Editor.PartialReadType.New)
                        {
                            DateTime tm = new DateTime(2000, 1, 1);
                            foreach (object[] it in rows)
                            {
                                if ((DateTime)it[0] > tm)
                                {
                                    tm = (DateTime)it[0];
                                }
                            }
                            partialRead.Start = tm.AddSeconds(1);
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }        

        public bool IsTransactionComplete(object sender, string command, GXPacket packet)
        {
            if (command == "IsAARQSend")
            {                
                bool complete = AARQRequest.Length == AARQRequestPos;
                return complete;
            }
            else if (command == "IsAllDataReceived")//Is all data read.
            {
                byte[] data = packet.ExtractPacket();
                CheckErrors(data, false);
                RequestTypes tmp = parser.GetDataFromPacket(data, ref ReceivedData);
                if (tmp == RequestTypes.None && IsMoreDataAvailable != RequestTypes.None)
                {
                    if ((IsMoreDataAvailable & RequestTypes.Frame) != 0)
                    {
                        IsMoreDataAvailable &= ~RequestTypes.Frame;
                    }
                    else
                    {
                        IsMoreDataAvailable &= ~RequestTypes.DataBlock;
                    }
                }
                else
                {
                    IsMoreDataAvailable |= tmp;
                }
                return IsMoreDataAvailable == Gurux.DLMS.RequestTypes.None;
            }
            else if (command == "IsAllTableInfoReceived")//Is all data read.
            {
                byte[] data = packet.ExtractPacket();
                CheckErrors(data, true);
                RequestTypes tmp = parser.GetDataFromPacket(data, ref ReceivedData);
                if (tmp == RequestTypes.None && IsMoreDataAvailable != RequestTypes.None)
                {
                    if ((IsMoreDataAvailable & RequestTypes.Frame) != 0)
                    {
                        IsMoreDataAvailable &= ~RequestTypes.Frame;
                    }
                    else
                    {
                        IsMoreDataAvailable &= ~RequestTypes.DataBlock;
                    }
                }
                else
                {
                    IsMoreDataAvailable |= tmp;
                }
                if (IsMoreDataAvailable != Gurux.DLMS.RequestTypes.None)
                {                    
                    return true;
                }
                ExtraInfo[ExtraInfo.Keys.ElementAt(ExtraInfoPos)] = parser.GetValue(ReceivedData);
                ReceivedData = null;
                //Read next item.
                if (++ExtraInfoPos < ExtraInfo.Count)
                {
                    return false;                    
                }                
                return true;                
            }                
            else if (command == "IsAllTableDataReceived")//Are all columns read.
            {                
                Gurux.Device.GXTable table = sender as Gurux.Device.GXTable;
                Gurux.Device.GXDevice device = table.Device;
                int progress = 0;                
                byte[] data = packet.ExtractPacket();
                CheckErrors(data, true);
                //Clear packet so we are not saved it for later use.
                packet.Clear();
                RequestTypes tmp = parser.GetDataFromPacket(data, ref ReceivedData);
                if (tmp == RequestTypes.None && IsMoreDataAvailable != RequestTypes.None)
                {
                    if ((IsMoreDataAvailable & RequestTypes.Frame) != 0)
                    {
                        IsMoreDataAvailable &= ~RequestTypes.Frame;
                    }
                    else
                    {
                        IsMoreDataAvailable &= ~RequestTypes.DataBlock;
                    }
                }
                else
                {
                    IsMoreDataAvailable |= tmp;
                }
                bool complete = IsMoreDataAvailable == Gurux.DLMS.RequestTypes.None;
                //Notify only percent to increase traffic.
                int maxValue = parser.GetMaxProgressStatus(ReceivedData);
                //Notify progess.
                if (maxValue == 0)
                {
                    progress = 0;
                }
                else
                {
                    double val = parser.GetCurrentProgressStatus(ReceivedData);                    
                    progress = (int)(val / maxValue * 100);
                }
                if (LastNotifiedTransactionProgress != progress)
                {
                    if (complete)
                    {
                        device.NotifyTransactionProgress(this, new Gurux.Device.GXTransactionProgressEventArgs(sender, 100, 100, Gurux.Device.DeviceStates.ReadEnd));
                    }
                    else
                    {
                        device.NotifyTransactionProgress(this, new Gurux.Device.GXTransactionProgressEventArgs(sender, progress, 100, Gurux.Device.DeviceStates.ReadStart));                        
                    }
                    LastNotifiedTransactionProgress = progress;
                }
                //Try Parse DLMS objexts.
                if (TryParse)
                {
                    GXDLMSTable table2 = sender as GXDLMSTable;
                    if (this.ReceivedRows == 0)
                    {
                        table2.ClearRows();                        
                    }                    
                    Array reply = (Array)parser.TryGetValue(ReceivedData);
                    // If there is data.
                    if (reply != null && reply.Length != 0)
                    {
                        int count = pg.Buffer.Count;
                        pg.SetValue(2, reply);                        
                        List<object> receivedData = new List<object>(pg.Buffer.ToArray());
                        //Remove old rows. We must keep latest because some meters returns null as date time.
                        if (count != 0)
                        {                            
                            receivedData.RemoveRange(0, count);
                            pg.Buffer.RemoveRange(0, count - 1);
                        }
                        List<object[]> rows = new List<object[]>(receivedData.Count);
                        bool updated = false;
                        if (Extension != null)
                        {
                            updated = Extension.UpdateTableData(TableColumns, table2, reply, rows);
                        }
                        if (!updated)
                        {
                            foreach (object row in receivedData)
                            {
                                List<object> cols = new List<object>();
                                foreach (int it in ColumnIndexs)
                                {
                                    cols.Add(((object[])row)[it]);
                                }
                                rows.Add(cols.ToArray());
                            }
                        }
                        table2.AddRows(-1, rows, false);
                        //Save latest read time. Note we must add Capture Period or we will read last values again.
                        Gurux.Device.Editor.IGXPartialRead partialRead = table as Gurux.Device.Editor.IGXPartialRead;
                        if (partialRead.Type == Gurux.Device.Editor.PartialReadType.New && rows[rows.Count - 1][0] is GXDateTime)
                        {                            
                            DateTime tm = (rows[rows.Count - 1][0] as GXDateTime).Value;
                            partialRead.Start = tm.AddSeconds(pg.CapturePeriod);
                        }
                        ReceivedRows += reply.Length;
                    }                  
                }
                return complete;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        Gurux.Device.GXProperty FindProperty(Gurux.Device.GXPropertyCollection Properties, string ln, int EdisType)
        {
            foreach (GXDLMSProperty it in Properties)
            {
                if (it.LogicalName == ln && it.AttributeOrdinal == EdisType)
                {
                    return it;
                }
            }
            return null;
        }

        Gurux.Device.GXProperty FindClockProperty(Gurux.Device.GXPropertyCollection Properties)
        {
            foreach (GXDLMSProperty it in Properties)
            {
                if (it.ObjectType == Gurux.DLMS.ObjectType.Clock)
                {
                    return it;
                }
            }
            return null;
        }

        public object UIValueToDeviceValue(Gurux.Device.GXProperty sender, object value)
        {
            return value;
        }

        #endregion
    }
}
