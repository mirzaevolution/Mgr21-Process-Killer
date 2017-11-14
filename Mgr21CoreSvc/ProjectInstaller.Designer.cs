namespace Mgr21CoreSvc
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Mgr21SvcProcInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.Mgr21SvcInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // Mgr21SvcProcInstaller
            // 
            this.Mgr21SvcProcInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.Mgr21SvcProcInstaller.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.Mgr21SvcInstaller});
            this.Mgr21SvcProcInstaller.Password = null;
            this.Mgr21SvcProcInstaller.Username = null;
            // 
            // Mgr21SvcInstaller
            // 
            this.Mgr21SvcInstaller.DisplayName = "Mgr21 Process Hunter";
            this.Mgr21SvcInstaller.ServiceName = "Mgr21CoreService";
            this.Mgr21SvcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.Mgr21SvcProcInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller Mgr21SvcProcInstaller;
        private System.ServiceProcess.ServiceInstaller Mgr21SvcInstaller;
    }
}