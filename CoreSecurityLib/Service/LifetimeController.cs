using System;
using System.ServiceProcess;
using CoreModel;
namespace CoreSecurityLib.Service
{
    public class LifetimeController
    {
        private DataResult<ServiceController> InitializeServiceController()
        {
            bool success = true;
            string error = "";
            ServiceController serviceController = null;
            try
            {
                serviceController = new ServiceController(Global.ServiceName);
            }
            catch (Exception ex)
            {
                success = false;
                error = $"An error occured while initializing service controller. Error: {ex.Message}";
            }
            return new DataResult<ServiceController>
            {
                Data = serviceController,
                MainResult = new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                }
            };
        }
        public MainResult StartService()
        {
            bool success = true;
            string error = "";
            DataResult<ServiceController> dataResult = InitializeServiceController();
            if (dataResult.MainResult.Success)
            {
                ServiceController serviceController = dataResult.Data;
               
                try
                {
                    if(serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    
                }
                catch(InvalidOperationException)
                {
                    success = false;
                    error = $"An error occured while starting the service. (Not Registered)";
                }
                catch (Exception ex)
                {
                    success = false;
                    error = $"An error occured while starting the service. Error: {ex.Message}";
                }
                return new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                };
            }
            else
                return dataResult.MainResult;
        }
        public MainResult StopService()
        {
            bool success = true;
            string error = "";
            DataResult<ServiceController> dataResult = InitializeServiceController();
            if (dataResult.MainResult.Success)
            {
                ServiceController serviceController = dataResult.Data;

                try
                {
                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    
                }
                catch (Exception ex)
                {
                    success = false;
                    error = $"An error occured while stopping the service. Error: {ex.Message}";
                }
                return new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                };
            }
            else
                return dataResult.MainResult;
        }
        public MainResult PauseService()
        {
            bool success = true;
            string error = "";
            DataResult<ServiceController> dataResult = InitializeServiceController();
            if (dataResult.MainResult.Success)
            {
                ServiceController serviceController = dataResult.Data;

                try
                {

                    if (serviceController.Status == ServiceControllerStatus.Running)
                    {
                        serviceController.Pause();
                        serviceController.WaitForStatus(ServiceControllerStatus.Paused);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    error = $"An error occured while pausing the service. Error: {ex.Message}";
                }
                return new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                };
            }
            else
                return dataResult.MainResult;
        }
        public MainResult ContinueService()
        {
            bool success = true;
            string error = "";
            DataResult<ServiceController> dataResult = InitializeServiceController();
            if (dataResult.MainResult.Success)
            {
                ServiceController serviceController = dataResult.Data;

                try
                {
                    if(serviceController.Status == ServiceControllerStatus.Paused)
                    {
                        serviceController.Continue();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running);
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    error = $"An error occured while continuing the service. Error: {ex.Message}";
                }
                
                return new MainResult
                {
                    Success = success,
                    ErrorMessage = error
                };
            }
            else
                return dataResult.MainResult;
        }
        public MainResult RefreshService()
        {
            MainResult pauseResult = PauseService();
            if(pauseResult.Success)
            {
                return ContinueService();
                
            }
            return pauseResult;
        }
    }
}
