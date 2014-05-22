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
using System.IO.Ports;

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
            return new Type[] { typeof(GXDLMSRegister) };
        }

		public override Type[] GetCategoryTypes(object parent)
        {
            return new Type[] { typeof(GXCategory), typeof(GXDLMSCategory) };
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

        internal byte[] ReadDataBlock(Gurux.DLMS.GXDLMSClient cosem, Gurux.Common.IGXMedia media, byte[] data, int wt, int scaler)
        {            
            byte[] reply = ReadDLMSPacket(cosem, media, data, wt);
            byte[] allData = null;
            RequestTypes moredata = cosem.GetDataFromPacket(reply, ref allData);
            int max = cosem.GetMaxProgressStatus(allData);
            int current = 0;
            while (moredata != 0)
            {
                while ((moredata & RequestTypes.Frame) != 0)
                {
                    data = cosem.ReceiverReady(RequestTypes.Frame);
                    reply = ReadDLMSPacket(cosem, media, data, wt);                    
                    if ((cosem.GetDataFromPacket(reply, ref allData) & RequestTypes.Frame) == 0)
                    {
                        current = cosem.GetCurrentProgressStatus(allData);
                        if (max != 0 && scaler != 0)
                        {
                            double tmp = current;
                            tmp /= max;
                            tmp *= max;
                            tmp += scaler * max;
                            Progress((int)tmp, 5 * max);
                        }
                        moredata &= ~RequestTypes.Frame;
                        break;
                    }
                    current = cosem.GetCurrentProgressStatus(allData);
                    if (max != 0 && scaler != 0)
                    {
                        double tmp = current;
                        tmp /= max;
                        tmp *= max;
                        tmp += scaler * max;
                        Progress((int)tmp, 5 * max);
                    }
                }
                if ((moredata & RequestTypes.DataBlock) != 0)
                {
                    //Send Receiver Ready.
                    data = cosem.ReceiverReady(RequestTypes.DataBlock);
                    reply = ReadDLMSPacket(cosem, media, data, wt);
                    moredata = cosem.GetDataFromPacket(reply, ref allData);
                    current = cosem.GetCurrentProgressStatus(allData);
                    if (max != 0 && scaler != 0)
                    {
                        double tmp = current;
                        tmp /= max;
                        tmp *= max;
                        tmp += scaler * max;
                        Progress((int)tmp, 5 * max);
                    }
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
                name = "0x" + Convert.ToString(it.ShortName, 16).PadLeft(4) + " " + it.LogicalName;
			}
            GXObisCode code = manufacturer.ObisCodes.FindByLN(it.ObjectType, it.LogicalName, null);
            if (code != null && !string.IsNullOrEmpty(code.Description))
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
        /// <param name="addinPages">Addin pages.</param>
        /// <param name="device">The target GXDevice to put imported items.</param>
		/// <param name="media">A media connection to the device.</param>
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
                    byte terminator = 0xA;
                    if (Device.StartProtocol == StartProtocolType.IEC)
                    {
                        GXSerial serial = media as GXSerial;
                        serial.Eop = terminator;
                        serial.Eop = terminator;
                        //Init IEC connection. This must done first with serial connections.
                        string str = "/?" + Device.SerialNumber + "!\r\n";
                        ReceiveParameters<string> args = new ReceiveParameters<string>()
                        {
                            Eop = terminator,
                            WaitTime = wt
                        };
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
                        int baudRate = 0;
                        switch (baudrate)
                        {
                            case '0':
                                baudRate = 300;
                                break;
                            case '1':
                                baudRate = 600;
                                break;
                            case '2':
                                baudRate = 1200;
                                break;
                            case '3':
                                baudRate = 2400;
                                break;
                            case '4':
                                baudRate = 4800;
                                break;
                            case '5':                            
                                baudRate = 9600;
                                break;
                            case '6':
                                baudRate = 19200;
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
                            //This is in standard. Do not remove sleep.
                            //Some meters work without it, but some do not.
                            System.Threading.Thread.Sleep(500);
                            serial.BaudRate = baudRate;
                            ReceiveParameters<byte[]> args2 = new ReceiveParameters<byte[]>()
                            {
                                Eop = terminator,
                                WaitTime = 100
                            };
                            //If this fails, just read all data.
                            if (!media.Receive(args2))
                            {
                                //Read buffer.
                                args2.AllData = true;
                                args2.WaitTime = 1;
                                media.Receive(args2);
                            }
                            serial.DataBits = 8;
                            serial.Parity = Parity.None;
                            serial.StopBits = StopBits.One;
                            serial.DiscardInBuffer();
                            serial.DiscardOutBuffer();
                            serial.ResetSynchronousBuffer();
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
                byte[] allData = null;
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
                //Now 1/5 or actions is done.
                Progress(1, 5);
                Trace("Read Objects\r\n");
                try
				{
                    allData = ReadDataBlock(cosem, media, cosem.GetObjectsRequest(), wt, 1);
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
				Trace("--- Read scalars ---\r\n");
				//Now 2/5 or actions is done.
                Progress(2 * max, 5 * max);
                GXCategory dataItems = new GXCategory();
                dataItems.Name = "Data Items";
                GXCategory registers = new GXCategory();
                registers.Name = "Registers";
                Device.Categories.Add(dataItems);
                Device.Categories.Add(registers);
                int pos = 0;
                foreach (GXDLMSObject it in objs)
                {
                    ++pos;
                    //Skip association views.
                    if (it.ObjectType == ObjectType.AssociationLogicalName ||
                        it.ObjectType == ObjectType.AssociationShortName)
                    {
                        continue;
                    }
                    if (it.ObjectType != ObjectType.ProfileGeneric)
                    {
                        object prop = UpdateData(media, Device, wt, cosem, man, it, dataItems, registers);
                        //Read scaler and unit
                        if (it.ObjectType == ObjectType.Register)
                        {
                            try
                            {
                                data = cosem.Read(it.Name, it.ObjectType, 3)[0];
                                allData = ReadDataBlock(cosem, media, data, wt, 2);
                                cosem.UpdateValue(allData, it, 3);
                                Gurux.DLMS.Objects.GXDLMSRegister item = it as Gurux.DLMS.Objects.GXDLMSRegister;
                                GXDLMSRegister r = prop as GXDLMSRegister;
                                r.Scaler = item.Scaler;
                                r.Unit = item.Unit.ToString();
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
                        }
                        //Read scaler and unit
                        else if (it.ObjectType == ObjectType.ExtendedRegister)
                        {
                            try
                            {
                                data = cosem.Read(it.Name, it.ObjectType, 3)[0];
                                allData = ReadDataBlock(cosem, media, data, wt, 2);
                                cosem.UpdateValue(allData, it, 3);
                                Gurux.DLMS.Objects.GXDLMSExtendedRegister item = it as Gurux.DLMS.Objects.GXDLMSExtendedRegister;
                                GXDLMSCategory cat = prop as GXDLMSCategory;
                                GXDLMSRegister r = cat.Properties[0] as GXDLMSRegister;
                                r.Scaler = item.Scaler;
                                r.Unit = item.Unit.ToString();
                                cat.Properties[1].SetValue(item.Scaler.ToString() + ", " + item.Unit.ToString(), true, PropertyStates.None);
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
                        }
                        //Read scaler and unit
                        else if (it.ObjectType == ObjectType.DemandRegister)
                        {
                            try
                            {
                                data = cosem.Read(it.Name, it.ObjectType, 3)[0];
                                allData = ReadDataBlock(cosem, media, data, wt, 2);
                                cosem.UpdateValue(allData, it, 3);
                                Gurux.DLMS.Objects.GXDLMSDemandRegister item = it as Gurux.DLMS.Objects.GXDLMSDemandRegister;
                                GXDLMSCategory cat = prop as GXDLMSCategory;
                                cat.Properties[2].SetValue(item.Scaler.ToString() + ", " + item.Unit.ToString(), true, PropertyStates.None);

                                GXDLMSRegister r = cat.Properties[0] as GXDLMSRegister;
                                r.Scaler = item.Scaler;
                                r.Unit = item.Unit.ToString();
                                r = cat.Properties[1] as GXDLMSRegister;
                                r.Scaler = item.Scaler;
                                r.Unit = item.Unit.ToString();
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
                        }                        
                    }
                    //Now 3/5 actions is done.
                    double tmp = pos * max; 
                    tmp /= max;
                    tmp += 2 * max;
                    Progress((int) tmp , 5 * max);
                }
                //Now 3/5 actions is done.
                Progress(3 * max, 5 * max);
                Trace("--- Read Generic profiles ---\r\n");
                GXDLMSObjectCollection pg = objs.GetObjects(ObjectType.ProfileGeneric);
                foreach (GXDLMSProfileGeneric it in pg)
                {
                    try
                    {
                        allData = ReadDataBlock(cosem, media, cosem.Read(it.Name, it.ObjectType, 3)[0], wt, 3);
                        cosem.UpdateValue(allData, it, 3);
                        UpdateData(media, Device, wt, cosem, man, it, dataItems, registers);
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
                //Now 4/5 actions is done.
                Progress(4 * max, 5 * max);            

                //Update IEC HDLC interval if found. 
                GXDLMSObjectCollection objects = objs.GetObjects(ObjectType.IecHdlcSetup);
                if (objects.Count != 0)
                {
                    allData = ReadDataBlock(cosem, media, cosem.Read(objects[0].Name, objects[0].ObjectType, 8)[0], wt, 5);
                    //Minus 10 second.
                    Device.Keepalive.Interval = (Convert.ToInt32(cosem.GetValue(allData)) - 10) * 1000;
                }

                //Now all actions are done.
                Progress(max, max);
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
                        if (!(media is GXNet && Device.SupportNetworkSpecificSettings))
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
        /* Mikko
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
        */

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
        }

        void UpdateObject(GXDLMSObject it, GXDLMSProperty item, int index)
        {
            //Update name.
            item.AttributeOrdinal = index;
            item.ShortName = it.ShortName;
            item.LogicalName = it.LogicalName;
            if (it.ShortName != 0)
            {
                item.Name = string.Format("0x{0} {1} {2}", it.ShortName.ToString("X4"), it.LogicalName, it.Description);
            }
            else
            {
                item.Name = it.LogicalName + " " + it.Description;
            }                        
            //Update description.
            item.Description = it.Description;
            //Update access mode.
            item.DLMSType = it.GetDataType(index);
            if (item.DLMSType == DataType.Enum)
            {
                item.ForcePresetValues = true;
                object value = it.GetValues()[index - 1];
                foreach (object val in Enum.GetValues(value.GetType()))
                {
                    item.Values.Add(new GXValueItem(val.ToString(), (int)val));
                }
            }
            else
            {
                item.ValueType = it.GetUIDataType(index);                
                if (item.ValueType == DataType.None)
                {
                    item.ValueType = it.GetDataType(index);                    
                }
            }
            item.AccessMode = (Gurux.Device.AccessMode)it.GetAccess(index);
        }

        void UpdateObject(GXDLMSObject it, GXDLMSCategory item)
        {
            item.ShortName = (UInt16)it.ShortName;
            item.LogicalName = it.LogicalName;
            if (it.ShortName != 0)
            {
                item.Name = string.Format("0x{0} {1} {2}", it.ShortName.ToString("X4"), it.LogicalName, it.Description);
            }
            else
            {
                item.Name = it.LogicalName + " " + it.Description;
            }
            //Update description.
            item.Description = it.Description;
            //Update atribute index.
            for (int pos = 2; pos != (it as IGXDLMSBase).GetAttributeCount() + 1; ++pos)
            {
                string name = (it as IGXDLMSBase).GetNames()[pos - 1];
                object value = it.GetValues()[pos - 1];
                GXDLMSProperty prop;
                if (((pos == 2 || pos == 3) && it.ObjectType == ObjectType.DemandRegister) || 
                    (pos == 2 && it.ObjectType == ObjectType.ExtendedRegister))
                {
                    prop = new GXDLMSRegister();
                    prop.ObjectType = it.ObjectType;
                    prop.LogicalName = it.LogicalName;
                    prop.ShortName = it.ShortName;
                    prop.Name = name;
                    prop.AttributeOrdinal = pos;
                }
                else
                {
                    prop = new GXDLMSProperty(it.ObjectType, it.LogicalName, it.ShortName, name, pos);
                }
                item.Properties.Add(prop);
                prop.DLMSType = it.GetDataType(pos);
                //Update scaler and unit.
                if ((pos == 4 && it.ObjectType == ObjectType.DemandRegister) ||
                    (pos == 3 && it.ObjectType == ObjectType.ExtendedRegister))
                {
                    prop.ValueType = DataType.String;
                }
                else
                {
                    if (value is Enum)
                    {
                        prop.ForcePresetValues = true;
                        foreach (object val in Enum.GetValues(value.GetType()))
                        {
                            prop.Values.Add(new GXValueItem(val.ToString(), (int)val));
                        }
                    }
                    else
                    {
                        prop.ValueType = it.GetUIDataType(pos);
                        if (prop.ValueType == DataType.None)
                        {
                            prop.ValueType = it.GetDataType(pos);
                        }
                    }
                }
                prop.AccessMode = (Gurux.Device.AccessMode)it.GetAccess(pos);
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
                UpdateObject(it, prop, 2);
                registers.Properties.Add(prop);
                return prop;
            }
            else if (it.ObjectType == Gurux.DLMS.ObjectType.Data)
            {
                GXDLMSProperty prop = new GXDLMSProperty();
                prop.ObjectType = ObjectType.Data;
                UpdateObject(it, prop, 2);
                dataItems.Properties.Add(prop);
                return prop;
            }
            else if (it.ObjectType == Gurux.DLMS.ObjectType.ProfileGeneric)
            {
                GXDLMSProfileGeneric pg = it as GXDLMSProfileGeneric;
                GXDLMSTable table = new GXDLMSTable();
                table.Name = it.LogicalName + " " + it.Description;
                table.ShortName = it.ShortName;
                table.LogicalName = it.LogicalName;
                table.AccessMode = Gurux.Device.AccessMode.Read;
                foreach(var it2 in pg.CaptureObjects)
                {
                    GXDLMSProperty prop;
                    if (it2.Key is Gurux.DLMS.Objects.GXDLMSRegister)
                    {
                        Gurux.DLMS.Objects.GXDLMSRegister tmp = it2.Key as Gurux.DLMS.Objects.GXDLMSRegister;
                        GXDLMSRegister r = new GXDLMSRegister();
                        prop = r;
                        r.Scaler = tmp.Scaler;
                        r.Unit = tmp.Unit.ToString();                    
                    }
                    else
                    {
                        prop = new GXDLMSProperty();
                    }
                    int index = it2.Value.AttributeIndex;
                    prop.Name = it2.Key.LogicalName + " " + it2.Key.Description;
                    prop.ObjectType = it2.Key.ObjectType;
                    prop.AttributeOrdinal = index;
                    prop.LogicalName = it2.Key.LogicalName;
                    table.Columns.Add(prop);
                    prop.DLMSType = it.GetDataType(index);                    
                    prop.ValueType = it2.Key.GetUIDataType(index);
                }
                Device.Tables.Add(table);
                return table; 
            }
            GXDLMSCategory cat = new GXDLMSCategory();
            cat.ObjectType = it.ObjectType;
            UpdateObject(it, cat);
            Device.Categories.Add(cat);
            return cat;            
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

        public override void InitializeAfterImport(GXDevice device)
        {            
            if (Gurux.DLMS.ManufacturerSettings.GXManufacturerCollection.IsFirstRun())
            {         
                Gurux.DLMS.ManufacturerSettings.GXManufacturerCollection.UpdateManufactureSettings();             
            }
        }
    }
}