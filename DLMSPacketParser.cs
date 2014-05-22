using System;
using System.Collections.Generic;
using System.Text;
using Gurux.Communication;
using Gurux.DLMS.AddIn.ManufacturerSettings;
using Gurux.Serial;
using Gurux.Net;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    public class DLMSPacketParser : IGXPacketParser
    {
        Gurux.DLMS.GXDLMSClient parser = new Gurux.DLMS.GXDLMSClient();
        GXDLMSDevice Device = null;
        #region IGXPacketParser Members

        /// <summary>
        /// Initialize settings.
        /// </summary>
        /// <param name="sender"></param>
        public void Connect(object sender)
        {
            GXClient client = sender as GXClient;
            Device = client.Owner as GXDLMSDevice;
            object ClientID = 0;
            switch (Device.Authentication)
            {
                case Gurux.DLMS.Authentication.High:
                    ClientID = Device.ClientIDHigh;
                    break;
                case Gurux.DLMS.Authentication.Low:
                    ClientID = Device.ClientIDLow;
                    break;
                case Gurux.DLMS.Authentication.None:
                    ClientID = Device.ClientID;
                    break;
            }
            //If network media is used check is manufacturer supporting IEC 62056-47
            if (!Device.UseRemoteSerial && Device.GXClient.Media is GXNet && Device.SupportNetworkSpecificSettings)
            {
                parser.InterfaceType = Gurux.DLMS.InterfaceType.Net;
                parser.ClientID = Convert.ToUInt16(Device.ClientID);
                parser.ServerID = Convert.ToUInt16(Device.PhysicalAddress);
            }
            else
            {
                if (Device.HDLCAddressing == HDLCAddressType.Custom)
                {
                    parser.ClientID = ClientID;
                }
                else
                {
                    parser.ClientID = (byte)(Convert.ToByte(ClientID) << 1 | 0x1);
                }
                if (Device.HDLCAddressing == HDLCAddressType.SerialNumber)
                {
                    parser.ServerID = GXManufacturer.CountServerAddress(Device.HDLCAddressing, Device.SNFormula, Convert.ToUInt32(Device.SerialNumber), Device.LogicalAddress);
                }
                else
                {
                    parser.ServerID = GXManufacturer.CountServerAddress(Device.HDLCAddressing, Device.SNFormula, Device.PhysicalAddress, Device.LogicalAddress);
                }
            }
            parser.UseLogicalNameReferencing = Device.UseLogicalNameReferencing;
        }

        public void Disconnect(object sender)
        {

        }

        public void BeforeSend(object sender, Gurux.Communication.GXPacket packet)
        {            
        }

        void IGXPacketParser.CountChecksum(object sender, GXChecksumEventArgs e)
        {
            throw new NotImplementedException();
        }

        void IGXPacketParser.IsReplyPacket(object sender, GXReplyPacketEventArgs e)
        {
            //Get data as byte array.            
            byte[] replyData = e.Received.ExtractPacket();
            if (!parser.IsDLMSPacketComplete(replyData))
            {
                e.Accept = false;
                //If echo
                byte[] sendData = e.Send.ExtractPacket();
                if (Gurux.Common.GXCommon.EqualBytes(sendData, replyData))
                {
                    e.Description = "Echo received.";
                }
            }
            else
            {
                byte[] sendData = e.Send.ExtractPacket();
                object[,] errors = parser.CheckReplyErrors(sendData, replyData);
                if (errors != null)
                {
                    e.Send.SenderInfo = errors[0, 1].ToString();
                    int error = (int)errors[0, 0];
                    throw new GXDLMSException(error);
                }
                else
                {                    
                    e.Accept = parser.IsReplyPacket(sendData, replyData);
                }
            }
        }

        /// <summary>
        /// DLMS no not send notifies. If notify is received it's old packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void IGXPacketParser.AcceptNotify(object sender, GXReplyPacketEventArgs e)
        {
            e.Accept = false;            
        }

        void IGXPacketParser.Load(object sender)
        {
           
        }

        void IGXPacketParser.ParsePacketFromData(object sender, GXParsePacketEventArgs e)
        {         
        }

        void IGXPacketParser.ReceiveData(object sender, GXReceiveDataEventArgs e)
        {            
        }

        void IGXPacketParser.VerifyPacket(object sender, GXVerifyPacketEventArgs e)
        {

        }        

        void IGXPacketParser.Received(object sender, GXReceivedPacketEventArgs e)
        {         
        }

        void IGXPacketParser.Unload(object sender)
        {            
        }

        public void InitializeMedia(object sender, Gurux.Common.IGXMedia media)
        {
            GXClient client = sender as GXClient;
            GXDLMSDevice dev = client.Owner as GXDLMSDevice;
            if (media is GXSerial)
            {
                GXSerial serial = media as GXSerial;
                serial.ConfigurableSettings = Gurux.Serial.AvailableMediaSettings.All;
                serial.Eop = (byte)0x7e;
                if (dev.StartProtocol == StartProtocolType.IEC)
                {
                    serial.BaudRate = 300;
                    serial.DataBits = 7;                    
                    serial.Parity = System.IO.Ports.Parity.Even;
                    serial.StopBits = System.IO.Ports.StopBits.One;
                }
                else
                {                    
                    serial.BaudRate = 9600;
                    serial.DataBits = 8;
                    serial.Parity = System.IO.Ports.Parity.None;
                    serial.StopBits = System.IO.Ports.StopBits.One;
                }
            }
            else if (media is GXNet)
            {
                if (dev.SupportNetworkSpecificSettings)
                {
                    client.Bop = client.Eop = null;
                    client.ChecksumSettings.Type = ChecksumType.None;
                }
                GXNet net = media as GXNet;
                net.Protocol = NetworkType.Tcp;
                net.Port = 4059;
                net.ConfigurableSettings = Gurux.Net.AvailableMediaSettings.Port | Gurux.Net.AvailableMediaSettings.Host;
            }
        }

        #endregion       
    }
}
