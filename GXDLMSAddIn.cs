//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL: svn://utopia/projects/GXDeviceEditor/Development/AddIns/DLMS/GXDLMS.cs $
//
// Version:         $Revision: 4103 $,
//                  $Date: 2011-09-27 09:38:09 +0300 (Tue, 27 Sep 2011) $
//                  $Author: kurumi $
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------


using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Reflection;
using Gurux.DLMS;

using Gurux.DLMS.AddIn.ManufacturerSettings;
using Gurux.Device.Editor;
using Gurux.Device;
using Gurux.Net;
using Gurux.Common;
using Gurux.Serial;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Objects;
using System.Text;

namespace Gurux.DLMS.AddIn
{
	/// <summary>
	/// Implements DLMS protocol addin component.
	/// </summary>    
    [GXCommunicationAttribute(typeof(DLMSPacketHandler), typeof(DLMSPacketParser))]
    public class GXDLMSAddIn : GXProtocolAddIn
	{
		/// <summary>
		/// Initializes a new instance of the GXDLMS class.
		/// </summary>
        public GXDLMSAddIn()
			: base("DLMS", false, true, false)
		{
			base.WizardAvailable = VisibilityItems.Categories | VisibilityItems.Tables | VisibilityItems.Properties | VisibilityItems.Device;
		}

        /// <summary>
        /// Use must import all data from DLMS/COSEM device.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override Functionalities GetFunctionalities(object target)
        {
            return Functionalities.Remove;
        }

        public override Type GetDeviceType()
        {
            return typeof(GXDLMSDevice);
        }

		public override Type[] GetPropertyTypes(object parent)
        {                        
            return new Type[] { typeof(GXDLMSData), typeof(GXDLMSRegister) };
        }

		public override Type[] GetCategoryTypes(object parent)
        {
            return new Type[] { typeof(GXCategory), typeof(GXDLMClock), typeof(GXDLMSExtendedRegister), typeof(GXDLMSDemandRegister), typeof(GXDLMSHdlcSetup), typeof(GXDLMSIECOpticalPortSetup) };
        }

		public override Type[] GetTableTypes(object parent)
        {
            return new Type[] { typeof(GXDLMSTable) };
        }

		public override Type[] GetExtraTypes(object parent)
        {
            return new Type[] { typeof(GXDLMSProperty), typeof(Gurux.DLMS.Authentication), typeof(StartProtocolType), typeof(Gurux.DLMS.DataType) };
        }

        public override Form GetCustomUI(object target)
        {
            if (target is GXDLMSDevice)
            {                
                return new GXDLMSDeviceUI(target as GXDLMSDevice);
            }
            return null;            
        }

        public override void ModifyWizardPages(object source, GXPropertyPageType type, System.Collections.Generic.List<Control> pages)
		{		        
            if (type == GXPropertyPageType.Import)
            {
                pages.Insert(1, new ImportSettings(source as GXDevice));
            }
            if (type == GXPropertyPageType.Device)
            {
                pages.Insert(1, new DlmsWizardPage1Dlg(source as GXDevice));
            }
            else if (type == GXPropertyPageType.Category)
            {
                if (source.GetType() != typeof(GXCategory))
                {
                    GXCategory cat = source as GXCategory;
                    pages.Insert(1, new DlmsTypeWizardDlg(cat.Device, source));
                }
            }
            else if (type == GXPropertyPageType.Table)                
            {
                GXTable table = source as GXTable;
                pages.Insert(1, new DlmsTypeWizardDlg(table.Device, source));
            }
            else if (type == GXPropertyPageType.Property)                
            {
                GXProperty p = source as GXProperty;
                pages.Insert(1, new DlmsTypeWizardDlg(p.Device, source));
            }            
		}
       
        public override VisibilityItems ItemVisibility
		{
			get
			{
				return VisibilityItems.Categories | VisibilityItems.Tables;
			}
		}

        internal byte[] ReadDLMSPacket(GXDLMSClient cosem, Gurux.Common.IGXMedia media, byte[] data, int wt)
		{
			if (data == null)
			{
				return null;
			}
            ReceiveParameters<byte[]> args = new ReceiveParameters<byte[]>()
            {
                Eop = (byte)0x7E,
                Count = 5,
                WaitTime = wt
            };
            if (cosem.InterfaceType == InterfaceType.Net)
            {
                args.Eop = null;
                args.Count = 8;
                args.AllData = true;
            }
            int pos = 0;
            bool succeeded = false;
            lock (media.Synchronous)
            {
                media.Send(data, null);
                while (!succeeded && pos != 3)
                {
                    succeeded = media.Receive(args);
                    if (!succeeded)
                    {
                        //Try to read again...
                        if (++pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data send failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }
                        string err = "Failed to receive reply from the device in given time.";
                        GXLogWriter.WriteLog(err, (byte[])args.Reply);
                        throw new Exception(err);
                    }
                }                
                //Loop until whole m_Cosem packet is received.                
                while (!(succeeded = cosem.IsDLMSPacketComplete(args.Reply)))
                {
                    if (!media.Receive(args))
                    {
                        //Try to read again...
                        if (++pos != 3)
                        {
                            System.Diagnostics.Debug.WriteLine("Data receive failed. Try to resend " + pos.ToString() + "/3");
                            continue;
                        }
                        string err = "Failed to receive reply from the device in given time.";
                        GXLogWriter.WriteLog(err, (byte[])args.Reply);
                        throw new Exception(err);
                    }
                }                
            }
            object[,] errors = cosem.CheckReplyErrors(data, args.Reply);
            if (errors != null)
            {
                int error = (int)errors[0, 0];
                throw new GXDLMSException(error);
            }
            return args.Reply;
		}

        internal byte[] ReadDataBlock(Gurux.DLMS.GXDLMSClient cosem, Gurux.Common.IGXMedia media, byte[] data, int wt)
        {            
            byte[] reply = ReadDLMSPacket(cosem, media, data, wt);
            byte[] allData = null;
            RequestTypes moredata = cosem.GetDataFromPacket(reply, ref allData);
            int maxProgress = cosem.GetMaxProgressStatus(allData);            
            while (moredata != 0)
            {
                while ((moredata & RequestTypes.Frame) != 0)
                {
                    data = cosem.ReceiverReady(RequestTypes.Frame);
                    reply = ReadDLMSPacket(cosem, media, data, wt);                    
                    if ((cosem.GetDataFromPacket(reply, ref allData) & RequestTypes.Frame) == 0)
                    {
                        moredata &= ~RequestTypes.Frame;
                        break;
                    }
                }
                if ((moredata & RequestTypes.DataBlock) != 0)
                {
                    //Send Receiver Ready.
                    data = cosem.ReceiverReady(RequestTypes.DataBlock);
                    reply = ReadDLMSPacket(cosem, media, data, wt);
                    moredata = cosem.GetDataFromPacket(reply, ref allData);
                }
            }
            return allData;
        }    

		string GetName(GXManufacturer manufacturer, bool useLN, GXDLMSObject it)
		{
			string name;            
            if (useLN)
			{
                name = it.LogicalName;
			}
			else
			{
				name = Convert.ToString(it.ShortName, 16);
				for (int pos = name.Length; pos < 4; ++pos)
				{
					name = "0" + name;
				}
                name = "0x" + name + " " + it.LogicalName;
			}
            GXObisCode code = manufacturer.ObisCodes.FindByLN(it.ObjectType, it.LogicalName, null);
            if (code != null)
            {
                name += " " + code.Description;
            }
            else
            {
                name += " " + it.Description;
            }
			return name;
		}

		/// <summary>
		/// Import properties from the device.
		/// </summary>
		/// <param name="device">The target GXDevice to put imported items.</param>
		/// <param name="trace">The trace text box.</param>
		/// <param name="media">A media connection to the device.</param>
		/// <param name="progressbar">A progressbar to show the progress of the import operation.</param>
		/// <returns>True if there were no errors, otherwise false.</returns>
        public override void ImportFromDevice(Control[] addinPages, GXDevice device, IGXMedia media)
		{
            media.Open();
			GXDLMSDevice Device = (GXDLMSDevice)device;            
			int wt = Device.WaitTime;
			GXDLMSClient cosem = null;
            byte[] data, reply = null;
            IGXManufacturerExtension Extension = null;
			try
			{                
				//Initialize connection.
                cosem = new GXDLMSClient();
                cosem.UseLogicalNameReferencing = Device.UseLogicalNameReferencing;				
                if (Device.Manufacturers == null)
                {
                    Device.Manufacturers = new GXManufacturerCollection();
                    GXManufacturerCollection.ReadManufacturerSettings(Device.Manufacturers);
                }
                GXManufacturer man = Device.Manufacturers.FindByIdentification(Device.Identification);
                if (!string.IsNullOrEmpty(man.Extension))
                {
                    Type t = Type.GetType(man.Extension);
                    Extension = Activator.CreateInstance(t) as IGXManufacturerExtension;
                }

                if (!Device.UseRemoteSerial && media is GXNet) //If media is network.
				{
                    if (Device.SupportNetworkSpecificSettings)
                    {
                        cosem.InterfaceType = Gurux.DLMS.InterfaceType.Net;
                    }
				}
				else if (media is GXSerial) //If media is serial.
				{
                    byte Terminator = 0xA;
                    if (Device.StartProtocol == StartProtocolType.IEC)
                    {
                        GXSerial serial = media as GXSerial;
                        serial.Eop = null;
                        //Init IEC connection. This must done first with serial connections.
                        string str = "/?!\r\n";                        
                        ReceiveParameters<string> args = new ReceiveParameters<string>();
                        args.Eop = Terminator;
                        args.WaitTime = wt;                        
                        lock (media.Synchronous)
                        {
                            media.Send(str, null);
                            do
                            {
                                args.Reply = null;                                
                                if (!media.Receive(args))
                                {
                                    throw new Exception("Failed to receive reply from the device in given time.");
                                }
                            }
                            while (str == args.Reply);//Remove echo
                        }
                        string answer = args.Reply.ToString();                        
                        if (answer[0] != '/')
                        {
                            throw new Exception("Invalid responce.");
                        }
                        string manufactureID = answer.Substring(1, 3);
                        char baudrate = answer[4];
                        if (baudrate == ' ')
                        {
                            baudrate = '5';
                        }
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
                        data = new byte[] { 0x06, controlCharacter, (byte)baudrate, ModeControlCharacter, 0x0D, 0x0A };                        
                        lock (media.Synchronous)
                        {
                            args.Reply = null;                            
                            media.Send(data, null);                                                       
                            ReceiveParameters<byte[]> args2 = new ReceiveParameters<byte[]>();
                            args2.Eop = (byte)0x0A;
                            args2.WaitTime = 500;
                            if (!media.Receive(args))
                            {
                                //Read buffer.
                                args.AllData = true;
                                args.WaitTime = 1;
                                media.Receive(args);
                            }
                            if (serial != null)
                            {
                                while (serial.BytesToWrite != 0)
                                {
                                    System.Threading.Thread.Sleep(50);
                                }
                                serial.RtsEnable = false;
                                serial.BaudRate = bitrate;
                                serial.DataBits = 8;
                                serial.Parity = System.IO.Ports.Parity.None;
                                serial.StopBits = System.IO.Ports.StopBits.One;
                                serial.RtsEnable = true;
                            }
                        }                        
                    }
				}
                media.Eop = (byte) 0x7E;
                cosem.Authentication = (Gurux.DLMS.Authentication)Device.Authentication;
                object clientAdd = null;
                if (cosem.Authentication == Authentication.None)
                {
                    clientAdd = Device.ClientID;
                }
                else if (cosem.Authentication == Authentication.Low)
                {
                    clientAdd = Device.ClientIDLow;
                }
                else if (cosem.Authentication == Authentication.High)
                {
                    clientAdd = Device.ClientIDHigh;
                }
                if (!string.IsNullOrEmpty(Device.Password))
                {
                    cosem.Password = ASCIIEncoding.ASCII.GetBytes(Device.Password);
                }
                else
                {
                    cosem.Password = null;
                }
                //If network media is used check is manufacturer supporting IEC 62056-47
                if (!Device.UseRemoteSerial && media is GXNet && Device.SupportNetworkSpecificSettings)
                {
                    cosem.InterfaceType = InterfaceType.Net;
                    media.Eop = null;
                    cosem.ClientID = Convert.ToUInt16(clientAdd);
                    cosem.ServerID = Convert.ToUInt16(Device.PhysicalAddress);
                }
                else
                {
                    if (Device.HDLCAddressing == HDLCAddressType.Custom)
                    {
                        cosem.ClientID = clientAdd;
                    }
                    else
                    {
                        cosem.ClientID = (byte)(Convert.ToByte(clientAdd) << 1 | 0x1);
                    }
                    if (Device.HDLCAddressing == HDLCAddressType.SerialNumber)
                    {
                        cosem.ServerID = GXManufacturer.CountServerAddress(Device.HDLCAddressing, Device.SNFormula, Convert.ToUInt32(Device.SerialNumber), Device.LogicalAddress);
                    }
                    else
                    {
                        cosem.ServerID = GXManufacturer.CountServerAddress(Device.HDLCAddressing, Device.SNFormula, Device.PhysicalAddress, Device.LogicalAddress);
                    }                    
                }
                object allData = null;
				data = cosem.SNRMRequest();
				//General Network connection don't need SNRMRequest.
				if (data != null)
				{
					Trace("--- Initialize DLMS connection\r\n");
					try
					{
						reply = ReadDLMSPacket(cosem, media, data, wt);
					}
					catch (Exception Ex)
					{
						throw new Exception("DLMS Initialize failed. " + Ex.Message);
					}
					//Has server accepted client.
					cosem.ParseUAResponse(reply);
				}
				Trace("Connecting\r\n");
                media.ResetSynchronousBuffer();
				try
				{                    
                    foreach (byte[] it in cosem.AARQRequest(null))
                    {                        
                        reply = ReadDLMSPacket(cosem, media, it, wt);
                    }
				}
				catch (Exception Ex)
				{
					throw new Exception("DLMS AARQRequest failed. " + Ex.Message);
				}
                cosem.ParseAAREResponse(reply);
				Trace("Read Objects\r\n");
                try
				{
                    allData = ReadDataBlock(cosem, media, cosem.GetObjectsRequest(), wt);
				}
				catch (Exception Ex)
				{
					throw new Exception("DLMS AARQRequest failed. " + Ex.Message);
				}               
                Trace("--- Parse Objects ---\r\n");
                GXDLMSObjectCollection objs = cosem.ParseObjects((byte[])allData, true);
			    allData = null;
				//Now we know exact number of read registers. Update progress bar again.
                int max = objs.Count;                				
				Trace("--- Read Generic profiles ---\r\n");
                GXDLMSObjectCollection pg = objs.GetObjects(ObjectType.ProfileGeneric);
				foreach (GXDLMSProfileGeneric it in pg)
				{
                    try
                    {
                        allData = ReadDataBlock(cosem, media, cosem.Read(it.Name, it.ObjectType, 3)[0], wt);
                        GXDLMSObjectCollection items = cosem.ParseColumns((byte[])allData);
                        allData = null;
                        it.CaptureObjects.AddRange(items);
                    }
                    //Ignore HW error and read next.
                    catch (GXDLMSException)
                    {
                        continue;
                    }
                    catch (Exception Ex)
                    {
                        Trace("DLMS Generic Profile read failed. " + Ex.Message + Environment.NewLine);
                    }
                }                
				Trace("--- Read scalars ---\r\n");
				//Now 1/2 or registers is read...
                Progress(2 * max, 4 * max);
                GXCategory dataItems = new GXCategory();
                dataItems.Name = "Data Items";
                GXCategory registers = new GXCategory();
                registers.Name = "Registers";
                Device.Categories.Add(dataItems);
                Device.Categories.Add(registers);
                //Scalers must read one by one because all devices do not support reading all at one message...
                GXDLMSObjectCollection regs = objs.GetObjects(new ObjectType[] { ObjectType.Register});
                foreach (GXDLMSObject it in objs)
                {
                    object prop = UpdateData(media, Device, wt, cosem, man, it, dataItems, registers);
                    if (it.ObjectType == ObjectType.Register || it.ObjectType == ObjectType.ExtendedRegister)
                    {
                        GXObisCode code = man.ObisCodes.FindByLN(it.ObjectType, it.LogicalName, null);
                        try
                        {
                            if (code != null)
                            {
                                GXDLMSAttributeSettings att = code.Attributes.Find(3);
                                if (att != null && att.Access == AccessMode.NoAccess)
                                {
                                    continue;
                                }
                            }
                            data = cosem.Read(it.Name, it.ObjectType, 3)[0];
                            allData = ReadDataBlock(cosem, media, data, wt);
                        }
                        //Ignore HW error and read next.
                        catch (GXDLMSException)
                        {
                            continue;
                        }
                        catch (Exception Ex)
                        {
                            throw new Exception("DLMS Register Scaler and Unit read failed. " + Ex.Message);
                        }
                        allData = (Array)cosem.GetValue((byte[])allData);
                        double Scaler = Math.Pow(10, Convert.ToInt32(((Array)allData).GetValue(0)));
                        GXDLMSRegister r = prop as GXDLMSRegister;
                        if (r != null)
                        {
                            if (Scaler != 1)
                            {
                                r.Scaler = Scaler;
                            }
                            r.Unit = GXDLMSClient.GetUnit((Unit)(Convert.ToInt32(((Array)allData).GetValue(1))));
                        }
                        allData = null;
                    }
                    Progress(2 * max, 4 * max);                    
                }
                //Now 3/4 or registers is read...
                Progress(3 * max, 4 * max);                
                //Update tables coluns.
                CreateTables(media, Device, man, wt, cosem, Extension, objs, dataItems, registers);
                try
                {
                    Device.Manufacturers.WriteManufacturerSettings();
                }
                catch
                {
                    //Skip errors.
                }
                Trace("--- Succeeded ---\r\n");
			}
			finally
			{                
                if (cosem != null && media != null)
				{
					Trace("--- Disconnecting ---\r\n");
                    byte[] allData = null;
					if (cosem != null)
					{
						//Network standard don't need this.					
						if (!Device.SupportNetworkSpecificSettings)
						{
							try
							{
								reply = ReadDLMSPacket(cosem, media, cosem.DisconnectRequest(), wt);
								cosem.GetDataFromPacket(reply, ref allData);
							}
							catch (Exception Ex)
							{
								Trace("DisconnectRequest failed. " + Ex.Message);
							}
						}
					}
					if (media != null)
					{
						media.Close();
						media = null;
					}
					Trace("--- Disconnected ---\r\n--- Done---\r\n");					
				}
			}			
		}

        private void CreateTables(Gurux.Common.IGXMedia media, GXDLMSDevice Device, GXManufacturer man, int wt, GXDLMSClient cosem, IGXManufacturerExtension Extension, GXDLMSObjectCollection objs, GXCategory dataItems, GXCategory registers)
        {
            //Profile generic will handle here, because it will need register objects and they must read fist.
            foreach (GXDLMSObject it in objs)
            {
                if (it.ObjectType == ObjectType.ProfileGeneric)
                {
                    GXDLMSTable table = new GXDLMSTable();
                    table.Name = it.LogicalName + " " + it.Description;
                    table.ShortName = it.ShortName;
                    table.LogicalName = it.LogicalName;
                    table.AccessMode = Gurux.Device.AccessMode.Read;
                    Device.Tables.Add(table);
                    System.Diagnostics.Debug.WriteLine("---- " + it.LogicalName);
                    if (Extension != null)
                    {
                        Extension.UpdateColumns(Device, man, cosem, media, wt, it, table, this, dataItems, registers);                        
                    }
                    else
                    {
                        CreateColumns(media, Device, man, wt, cosem, Extension, dataItems, registers, it, table);
                    }
                }                
            }
        }

        string GetName(GXManufacturer man, Gurux.DLMS.ObjectType type, string logicanName)
        {
            GXObisCode code = man.ObisCodes.FindByLN(type, logicanName, null);
            string name = null;
            if (code != null)
            {
                name = code.Description;
            }
            if (string.IsNullOrEmpty(name))
            {
                name = logicanName;
            }
            return name;
        }

        internal void CreateColumns(Gurux.Common.IGXMedia media, GXDLMSDevice Device, GXManufacturer man, int wt, GXDLMSClient cosem, IGXManufacturerExtension Extension, GXCategory dataItems, GXCategory registers, GXDLMSObject it, GXDLMSTable table)
        {
            foreach (GXDLMSObject col in ((GXDLMSProfileGeneric)it).CaptureObjects)
            {
                try
                {
                    int index = col.SelectedAttributeIndex;
                    //Get default index if not given.
                    if (index == 0)
                    {
                        index = 2;
                    }
                    if (col.ObjectType == ObjectType.Register)
                    {
                        GXDLMSRegister r = FindByLN(registers.Properties, typeof(GXDLMSRegister), col.LogicalName) as GXDLMSRegister;
                        if (r == null)
                        {
                            r = new GXDLMSRegister();
                            r.LogicalName = col.LogicalName;
                            r.ObjectType = col.ObjectType;
                            r.Version = col.Version;
                            r.Name = GetName(man, Gurux.DLMS.ObjectType.Register, col.LogicalName);
                        }
                        GetDataType(man, cosem, media, wt, r, index);
                        r.AttributeOrdinal = index;
                        //If Attribute Ordinal is Event type
                        if ((index & 0x8) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Event Status";
                        }
                        //If Attribute Ordinal is is status
                        else if ((index & 0x10) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Status";
                        }
                        //If Edis type is normal data.
                        else
                        {
                            GetDataType(man, cosem, media, wt, r, index);
                        }
                        table.Columns.Add(r.Clone());
                    }
                    else if (col.ObjectType == ObjectType.DemandRegister)
                    {
                        GXDLMSProperty r = FindByLN(Device.Categories, typeof(GXDLMSDemandRegister), col.LogicalName, index) as GXDLMSProperty;
                        if (r == null)
                        {
                            r = new GXDLMSProperty();
                            r.LogicalName = col.LogicalName;
                            r.ObjectType = col.ObjectType;
                            r.Version = col.Version;
                            r.Name = GetName(man, Gurux.DLMS.ObjectType.DemandRegister, col.LogicalName);
                        }
                        GetDataType(man, cosem, media, wt, r, index);
                        r.AttributeOrdinal = index;
                        //If Attribute Ordinal is Event type
                        if ((index & 0x8) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Event Status";
                        }
                        //If Attribute Ordinal is is status
                        else if ((index & 0x10) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Status";
                        }
                        //If Edis type is normal data.
                        else
                        {
                            GetDataType(man, cosem, media, wt, r, index);
                        }
                        table.Columns.Add(r.Clone());
                    }
                    else if (col.ObjectType == ObjectType.ExtendedRegister)
                    {                        
                        GXDLMSProperty r = FindByLN(Device.Categories, typeof(GXDLMSExtendedRegister), col.LogicalName, index) as GXDLMSProperty;
                        if (r == null)
                        {
                            r = new GXDLMSProperty();
                            r.LogicalName = col.LogicalName;
                            r.ObjectType = col.ObjectType;
                            r.Version = col.Version;
                            r.Name = GetName(man, Gurux.DLMS.ObjectType.ExtendedRegister, col.LogicalName);
                        }
                        GetDataType(man, cosem, media, wt, r, index);
                        r.AttributeOrdinal = index;
                        //If Attribute Ordinal is Event type
                        if ((index & 0x8) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Event Status";
                        }
                        //If Attribute Ordinal is is status
                        else if ((index & 0x10) != 0)
                        {
                            r.Name = col.LogicalName + " " + "Status";
                        }
                        //If Edis type is normal data.
                        else
                        {
                            GetDataType(man, cosem, media, wt, r, index);
                        }
                        table.Columns.Add(r.Clone());
                    }
                    else if (col.ObjectType == ObjectType.Data)
                    {
                        GXDLMSData d = FindByLN(dataItems.Properties, typeof(GXDLMSData), col.LogicalName) as GXDLMSData;
                        if (d == null)
                        {
                            d = new GXDLMSData();
                            d.LogicalName = col.LogicalName;
                            d.ObjectType = col.ObjectType;
                            d.Version = col.Version;
                            d.Name = GetName(man, Gurux.DLMS.ObjectType.Data, col.LogicalName);
                        }
                        GetDataType(man, cosem, media, wt, d, index);
                        d.AttributeOrdinal = index;
                        table.Columns.Add(d.Clone());
                    }
                    else if (col.ObjectType == ObjectType.Clock)
                    {
                        GXDLMSProperty prop = FindByLN(Device.Categories, typeof(GXDLMClock), col.LogicalName, index) as GXDLMSProperty;
                        if (prop == null)
                        {
                            prop = new GXDLMSProperty();
                            prop.LogicalName = col.LogicalName;
                            prop.ObjectType = col.ObjectType;
                            prop.Version = col.Version;
                            prop.Name = GetName(man, Gurux.DLMS.ObjectType.Clock, col.LogicalName);
                        }                        
                        GetDataType(man, cosem, media, wt, prop, index);                        
                        table.Columns.Add(prop.Clone());
                    }
                    else if (col.ObjectType == ObjectType.ProfileGeneric)
                    {
                        //Profile Generics are taken care of later.
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Unknown column type: " + col.ObjectType.ToString());
                        Trace("Unknown column type: " + col.ObjectType.ToString() + "\r\n");
                    }
                }
                //Ignore HW error and read next.
                catch (GXDLMSException)
                {
                    continue;
                }
                catch (Exception Ex)
                {
                    Trace(Ex.ToString());
                    continue;
                }
            }
        }

        /// <summary>
        /// Update categories data.
        /// </summary>
        /// <param name="trace"></param>
        /// <param name="progressbar"></param>
        /// <param name="media"></param>
        /// <param name="Device"></param>
        /// <param name="wt"></param>
        /// <param name="cosem"></param>
        /// <param name="man"></param>
        /// <param name="objs"></param>
        /// <param name="dataItems"></param>
        /// <param name="registers"></param>
        private object UpdateData(Gurux.Common.IGXMedia media, GXDLMSDevice Device, int wt, GXDLMSClient cosem, GXManufacturer man, GXDLMSObject it, GXCategory dataItems, GXCategory registers)
        {
            GXObisCode code = man.ObisCodes.FindByLN(it.ObjectType, it.LogicalName, null);
            if (it.ObjectType == ObjectType.Register)
            {
                GXDLMSRegister prop = new GXDLMSRegister();
                prop.AccessMode = (Gurux.Device.AccessMode)it.GetAccess(2);
                prop.LogicalName = it.LogicalName;
                if (code != null)
                {
                    prop.Description = code.Description;
                }
                else
                {
                    prop.Description = it.Description;
                }
                prop.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                prop.ShortName = (UInt16)it.ShortName;
                //Registers must read to define data type. This must done because DLMS don't tell data type.                        
                try
                {
                    if (code != null && code.UIType != DataType.None)
                    {
                        prop.ValueType = GXObisCode.GetDataType(code.UIType);
                    }
                    else if (code != null)
                    {
                        prop.DLMSType = code.Type;
                    }
                    if (prop.DLMSType == DataType.None)
                    {
                        GetDataType(man, cosem, media, wt, prop, 2);
                    }
                    //Don't add the property if data type is unknown.
                    if (prop.DLMSType == DataType.None ||
                        prop.DLMSType == DataType.Array)
                    {
                        return null;
                    }
                }
                //Ignore HW error and read next.
                catch (GXDLMSException)
                {
                    return null;
                }
                catch (Exception Ex)
                {
                    Trace("GetDataType failed for register: " + prop.Name + "\r\n" + Ex.ToString());
                    return null;
                }
                registers.Properties.Add(prop);
                return prop;
            }
            else if (it.ObjectType == Gurux.DLMS.ObjectType.Data)
            {
                GXDLMSData prop = new GXDLMSData();
                try
                {
                    prop.LogicalName = it.LogicalName;
                    prop.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                    prop.ShortName = (UInt16)it.ShortName;
                    if (code != null && code.UIType != DataType.None)
                    {
                        prop.ValueType = GXObisCode.GetDataType(code.UIType);
                    }
                    else if (code != null)
                    {
                        prop.DLMSType = code.Type;
                    }
                    if (prop.DLMSType == DataType.None)
                    {
                        GetDataType(man, cosem, media, wt, prop, 2);
                    }
                    //Don't add the property if data type is unknown.
                    if (prop.DLMSType == DataType.None ||
                        prop.DLMSType == DataType.Array)
                    {
                        return null;
                    }
                    prop.Initialize(code);
                }
                //Ignore HW error and read next.
                catch (GXDLMSException)
                {
                    return null;
                }
                catch (Exception Ex)
                {
                    Trace("GetDataType failed for register: " + prop.Name + "\r\n" + Ex.ToString());
                    return null;
                }
                dataItems.Properties.Add(prop);
                return prop;
            }
            else if (it.ObjectType == Gurux.DLMS.ObjectType.ExtendedRegister)
            {
                GXDLMSExtendedRegister item = new GXDLMSExtendedRegister();
                item.ShortName = (UInt16)it.ShortName;
                item.LogicalName = it.LogicalName;
                item.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                Device.Categories.Add(item);
                item.Initialize(code);
                Device.Categories.Add(item);
                return item;
            }
            else if (it.ObjectType == Gurux.DLMS.ObjectType.DemandRegister)
            {
                GXDLMSDemandRegister item = new GXDLMSDemandRegister();
                item.ShortName = (UInt16)it.ShortName;
                item.LogicalName = it.LogicalName;
                item.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                Device.Categories.Add(item);
                item.Initialize(code);
                Device.Categories.Add(item);
                return item;
            }
            else if (it.ObjectType == ObjectType.Clock)
            {
                GXDLMClock item = new GXDLMClock();
                item.ShortName = (UInt16)it.ShortName;
                item.LogicalName = it.LogicalName;
                item.Name = it.LogicalName + " Clock";
                Device.Categories.Add(item);
                item.Initialize(code);
                Device.Categories.Add(item);
                return item;
            }
            else if (it.ObjectType == ObjectType.IecHdlcSetup)
            {
                GXDLMSHdlcSetup item = new GXDLMSHdlcSetup();
                item.ShortName = (UInt16)it.ShortName;
                item.LogicalName = it.LogicalName;
                item.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                Device.Categories.Add(item);
                item.Initialize(code);
                Device.Categories.Add(item);
                return item;
            }
            else if (it.ObjectType == ObjectType.IecLocalPortSetup)
            {
                GXDLMSIECOpticalPortSetup item = new GXDLMSIECOpticalPortSetup();
                item.ShortName = (UInt16)it.ShortName;
                item.LogicalName = it.LogicalName;
                item.Name = GetName(man, cosem.UseLogicalNameReferencing, it);
                Device.Categories.Add(item);
                item.Initialize(code);
                Device.Categories.Add(item);
                return item;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Unknown Interface type: " + it.ObjectType.ToString());
                Trace("Unknown Interface type: " + it.ObjectType.ToString() + "\r\n");
            }
            return null;
        }

        GXDLMSProperty FindByLN(GXPropertyCollection properties, Type type, string ln)
        {
            foreach (GXDLMSProperty it in properties)
            {
                if ((type == null || it.GetType() == type) && it.LogicalName == ln)
                {
                    return it;
                }
            }
            return null;
        }

        GXDLMSProperty FindByLN(GXCategoryCollection categories, Type type, string ln, int attributeOrder)
        {
            foreach (GXCategory it in categories)
            {
                if (it.GetType() == type && ((GXDLMSCategory)it).LogicalName == ln)
                {
                    foreach (GXDLMSProperty prop in it.Properties)
                    {
                        if (prop.AttributeOrdinal == attributeOrder)
                        {
                            return prop;
                        }                        
                    }
                }
            }
            return null;
        }

        void GetDataType(GXManufacturer man, GXDLMSClient cosem, Gurux.Common.IGXMedia media, int wt, GXDLMSProperty prop, int AttributeOrder)
        {
            //If value type is know.
            if (prop.ValueType != null)
            {
                return;
            }
            GXObisCode code = man.ObisCodes.FindByLN(prop.ObjectType, prop.LogicalName, null);
            if (code != null)
            {
                GXDLMSAttributeSettings att = code.Attributes.Find(AttributeOrder);
                if (att != null)
                {
                    prop.DLMSType = att.Type;
                    prop.ValueType = GXObisCode.GetDataType(att.UIType);
                    if (prop.DLMSType != DataType.None)
                    {
                        return;
                    }
                }
            }
            byte[] data = null;
            Gurux.DLMS.ObjectType objectType = ObjectType.None;
			object name;
            if (cosem.UseLogicalNameReferencing)
            {                
                objectType = prop.ObjectType;
				name = prop.LogicalName;
            }
			else
			{
				name = prop.ShortName;
			}
            try
            {
                data = cosem.Read(name, objectType, AttributeOrder)[0];
                byte[] allData = ReadDataBlock(cosem, media, data, wt);
                object vald = cosem.GetValue(allData);
                prop.DLMSType = cosem.GetDLMSDataType(allData);
                if (prop.DLMSType == DataType.OctetString)
                {
                    bool isAlpha = true;
                    object val = cosem.GetValue(allData);
                    for (int pos = 0; pos != ((Array)val).Length; ++pos)
                    {
                        byte ch = Convert.ToByte(((Array)val).GetValue(pos));
                        if (ch == 0xFF || !(char.IsLetterOrDigit((char)ch) || ch == 0))
                        {
                            isAlpha = false;
                            break;
                        }
                    }
                    //Get Value.
                    if (isAlpha)
                    {
                        prop.ValueType = typeof(string);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);                
            }
        }


        public override void InitializeAfterImport(GXDevice device)
        {            
            if (Gurux.DLMS.ManufacturerSettings.GXManufacturerCollection.IsFirstRun())
            {         
                Gurux.DLMS.ManufacturerSettings.GXManufacturerCollection.UpdateManufactureSettings();             
            }
        }
    }
}