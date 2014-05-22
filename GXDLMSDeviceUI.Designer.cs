namespace Gurux.DLMS.AddIn
{
    partial class GXDLMSDeviceUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.StartProtocolLbl = new System.Windows.Forms.Label();
            this.UseRemoteSerialCB = new System.Windows.Forms.CheckBox();
            this.AuthenticationGB = new System.Windows.Forms.GroupBox();
            this.passwordTb = new System.Windows.Forms.TextBox();
            this.passwordLbl = new System.Windows.Forms.Label();
            this.authLevelCb = new System.Windows.Forms.ComboBox();
            this.authLevelLbl = new System.Windows.Forms.Label();
            this.AddressingGB = new System.Windows.Forms.GroupBox();
            this.LogicalAddressTB = new System.Windows.Forms.NumericUpDown();
            this.LogicalAddressLbl = new System.Windows.Forms.Label();
            this.AddressTypeCB = new System.Windows.Forms.ComboBox();
            this.ServerIDTypeLbl = new System.Windows.Forms.Label();
            this.PhysicalAddressTB = new System.Windows.Forms.NumericUpDown();
            this.PhysicalAddressLbl = new System.Windows.Forms.Label();
            this.StartProtocolCB = new System.Windows.Forms.ComboBox();
            this.AuthenticationGB.SuspendLayout();
            this.AddressingGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogicalAddressTB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhysicalAddressTB)).BeginInit();
            this.SuspendLayout();
            // 
            // StartProtocolLbl
            // 
            this.StartProtocolLbl.AutoSize = true;
            this.StartProtocolLbl.Location = new System.Drawing.Point(8, 180);
            this.StartProtocolLbl.Name = "StartProtocolLbl";
            this.StartProtocolLbl.Size = new System.Drawing.Size(74, 13);
            this.StartProtocolLbl.TabIndex = 1;
            this.StartProtocolLbl.Text = "Start Protocol:";
            // 
            // UseRemoteSerialCB
            // 
            this.UseRemoteSerialCB.AutoSize = true;
            this.UseRemoteSerialCB.Location = new System.Drawing.Point(11, 204);
            this.UseRemoteSerialCB.Name = "UseRemoteSerialCB";
            this.UseRemoteSerialCB.Size = new System.Drawing.Size(117, 17);
            this.UseRemoteSerialCB.TabIndex = 2;
            this.UseRemoteSerialCB.Text = "Use Remote Serial:";
            this.UseRemoteSerialCB.UseVisualStyleBackColor = true;
            // 
            // AuthenticationGB
            // 
            this.AuthenticationGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthenticationGB.Controls.Add(this.passwordTb);
            this.AuthenticationGB.Controls.Add(this.passwordLbl);
            this.AuthenticationGB.Controls.Add(this.authLevelCb);
            this.AuthenticationGB.Controls.Add(this.authLevelLbl);
            this.AuthenticationGB.Location = new System.Drawing.Point(0, 0);
            this.AuthenticationGB.Name = "AuthenticationGB";
            this.AuthenticationGB.Size = new System.Drawing.Size(203, 78);
            this.AuthenticationGB.TabIndex = 14;
            this.AuthenticationGB.TabStop = false;
            this.AuthenticationGB.Text = "Authentication";
            // 
            // passwordTb
            // 
            this.passwordTb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.passwordTb.Enabled = false;
            this.passwordTb.Location = new System.Drawing.Point(107, 43);
            this.passwordTb.Name = "passwordTb";
            this.passwordTb.PasswordChar = '*';
            this.passwordTb.Size = new System.Drawing.Size(93, 20);
            this.passwordTb.TabIndex = 3;
            // 
            // passwordLbl
            // 
            this.passwordLbl.Enabled = false;
            this.passwordLbl.Location = new System.Drawing.Point(11, 43);
            this.passwordLbl.Name = "passwordLbl";
            this.passwordLbl.Size = new System.Drawing.Size(80, 24);
            this.passwordLbl.TabIndex = 17;
            this.passwordLbl.Text = "Password";
            // 
            // authLevelCb
            // 
            this.authLevelCb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.authLevelCb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.authLevelCb.Location = new System.Drawing.Point(107, 19);
            this.authLevelCb.Name = "authLevelCb";
            this.authLevelCb.Size = new System.Drawing.Size(93, 21);
            this.authLevelCb.TabIndex = 2;
            // 
            // authLevelLbl
            // 
            this.authLevelLbl.Location = new System.Drawing.Point(11, 19);
            this.authLevelLbl.Name = "authLevelLbl";
            this.authLevelLbl.Size = new System.Drawing.Size(80, 16);
            this.authLevelLbl.TabIndex = 14;
            this.authLevelLbl.Text = "Level";
            // 
            // AddressingGB
            // 
            this.AddressingGB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AddressingGB.Controls.Add(this.LogicalAddressTB);
            this.AddressingGB.Controls.Add(this.LogicalAddressLbl);
            this.AddressingGB.Controls.Add(this.AddressTypeCB);
            this.AddressingGB.Controls.Add(this.ServerIDTypeLbl);
            this.AddressingGB.Controls.Add(this.PhysicalAddressTB);
            this.AddressingGB.Controls.Add(this.PhysicalAddressLbl);
            this.AddressingGB.Location = new System.Drawing.Point(0, 68);
            this.AddressingGB.Name = "AddressingGB";
            this.AddressingGB.Size = new System.Drawing.Size(203, 96);
            this.AddressingGB.TabIndex = 33;
            this.AddressingGB.TabStop = false;
            this.AddressingGB.Text = "Addressing";
            // 
            // LogicalAddressTB
            // 
            this.LogicalAddressTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LogicalAddressTB.Hexadecimal = true;
            this.LogicalAddressTB.Location = new System.Drawing.Point(107, 72);
            this.LogicalAddressTB.Maximum = new decimal(new int[] {
            0,
            1,
            0,
            0});
            this.LogicalAddressTB.Name = "LogicalAddressTB";
            this.LogicalAddressTB.Size = new System.Drawing.Size(90, 20);
            this.LogicalAddressTB.TabIndex = 6;
            this.LogicalAddressTB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LogicalAddressLbl
            // 
            this.LogicalAddressLbl.AutoSize = true;
            this.LogicalAddressLbl.Location = new System.Drawing.Point(11, 74);
            this.LogicalAddressLbl.Name = "LogicalAddressLbl";
            this.LogicalAddressLbl.Size = new System.Drawing.Size(85, 13);
            this.LogicalAddressLbl.TabIndex = 24;
            this.LogicalAddressLbl.Text = "Logical Address:";
            // 
            // AddressTypeCB
            // 
            this.AddressTypeCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.AddressTypeCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AddressTypeCB.FormattingEnabled = true;
            this.AddressTypeCB.ItemHeight = 13;
            this.AddressTypeCB.Location = new System.Drawing.Point(107, 19);
            this.AddressTypeCB.Name = "AddressTypeCB";
            this.AddressTypeCB.Size = new System.Drawing.Size(93, 21);
            this.AddressTypeCB.TabIndex = 4;
            // 
            // ServerIDTypeLbl
            // 
            this.ServerIDTypeLbl.AutoSize = true;
            this.ServerIDTypeLbl.Location = new System.Drawing.Point(11, 22);
            this.ServerIDTypeLbl.Name = "ServerIDTypeLbl";
            this.ServerIDTypeLbl.Size = new System.Drawing.Size(34, 13);
            this.ServerIDTypeLbl.TabIndex = 21;
            this.ServerIDTypeLbl.Text = "Type:";
            // 
            // PhysicalAddressTB
            // 
            this.PhysicalAddressTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PhysicalAddressTB.Hexadecimal = true;
            this.PhysicalAddressTB.Location = new System.Drawing.Point(107, 46);
            this.PhysicalAddressTB.Maximum = new decimal(new int[] {
            0,
            1,
            0,
            0});
            this.PhysicalAddressTB.Name = "PhysicalAddressTB";
            this.PhysicalAddressTB.Size = new System.Drawing.Size(90, 20);
            this.PhysicalAddressTB.TabIndex = 5;
            this.PhysicalAddressTB.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // PhysicalAddressLbl
            // 
            this.PhysicalAddressLbl.AutoSize = true;
            this.PhysicalAddressLbl.Location = new System.Drawing.Point(11, 48);
            this.PhysicalAddressLbl.Name = "PhysicalAddressLbl";
            this.PhysicalAddressLbl.Size = new System.Drawing.Size(90, 13);
            this.PhysicalAddressLbl.TabIndex = 8;
            this.PhysicalAddressLbl.Text = "Physical Address:";
            // 
            // StartProtocolCB
            // 
            this.StartProtocolCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StartProtocolCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StartProtocolCB.Location = new System.Drawing.Point(104, 173);
            this.StartProtocolCB.Name = "StartProtocolCB";
            this.StartProtocolCB.Size = new System.Drawing.Size(93, 21);
            this.StartProtocolCB.TabIndex = 18;
            // 
            // GXDLMSDeviceUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(214, 226);
            this.Controls.Add(this.StartProtocolCB);
            this.Controls.Add(this.AddressingGB);
            this.Controls.Add(this.AuthenticationGB);
            this.Controls.Add(this.UseRemoteSerialCB);
            this.Controls.Add(this.StartProtocolLbl);
            this.Name = "GXDLMSDeviceUI";
            this.Text = "GXDLMSDeviceUI";
            this.AuthenticationGB.ResumeLayout(false);
            this.AuthenticationGB.PerformLayout();
            this.AddressingGB.ResumeLayout(false);
            this.AddressingGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LogicalAddressTB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PhysicalAddressTB)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label StartProtocolLbl;
        private System.Windows.Forms.CheckBox UseRemoteSerialCB;
        private System.Windows.Forms.GroupBox AuthenticationGB;
        private System.Windows.Forms.TextBox passwordTb;
        private System.Windows.Forms.Label passwordLbl;
        private System.Windows.Forms.ComboBox authLevelCb;
        private System.Windows.Forms.Label authLevelLbl;
        private System.Windows.Forms.GroupBox AddressingGB;
        private System.Windows.Forms.NumericUpDown LogicalAddressTB;
        private System.Windows.Forms.Label LogicalAddressLbl;
        private System.Windows.Forms.ComboBox AddressTypeCB;
        private System.Windows.Forms.Label ServerIDTypeLbl;
        private System.Windows.Forms.NumericUpDown PhysicalAddressTB;
        private System.Windows.Forms.Label PhysicalAddressLbl;
        private System.Windows.Forms.ComboBox StartProtocolCB;

    }
}