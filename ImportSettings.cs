using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Gurux.Common;
using Gurux.Device;
using GXDLMS.ManufacturerSettings;
using Gurux.DLMS.ManufacturerSettings;

namespace Gurux.DLMS.AddIn
{
    public partial class ImportSettings : Form, IGXWizardPage
    {
        GXDLMSDevice m_device;
        public ImportSettings(GXDevice device)
        {
            InitializeComponent();
            m_device = device as GXDLMSDevice;
            PasswordTB.ReadOnly = m_device.Authentication == Gurux.DLMS.Authentication.None;
            SerialNumberTB.ReadOnly = m_device.HDLCAddressing != HDLCAddressType.SerialNumber;
            PasswordTB.Text = m_device.Password;
            SerialNumberTB.Text = m_device.SerialNumber;
        }
       
        
        #region IGXWizardPage Members

        bool IGXWizardPage.IsShown()
        {
            return m_device.StartProtocol == StartProtocolType.IEC || m_device.Authentication != Authentication.None;
        }

        void IGXWizardPage.Next()
        {
            if (m_device.Authentication != Gurux.DLMS.Authentication.None)
            {
                m_device.Password = PasswordTB.Text;
            }
            if (m_device.HDLCAddressing == HDLCAddressType.SerialNumber)
            {
                m_device.SerialNumber = SerialNumberTB.Text;
                if (SerialNumberTB.Text.Length == 0)
                {
                    throw new Exception("Invalid Serial Number.");
                }
            }
        }

        void IGXWizardPage.Back()
        {
            
        }

        void IGXWizardPage.Finish()
        {
            
        }

        void IGXWizardPage.Cancel()
        {            
        }

        void IGXWizardPage.Initialize()
        {
            
        }

        GXWizardButtons IGXWizardPage.EnabledButtons
        {
            get
            {
                return GXWizardButtons.All;
            }
        }        

        string IGXWizardPage.Caption
        {
            get
            {
                return "";
            }
        }

        string IGXWizardPage.Description
        {
            get
            {
                return "";
            }
        }
        object IGXWizardPage.Target
        {
            get;
            set;            
        }
        #endregion
    }
}
