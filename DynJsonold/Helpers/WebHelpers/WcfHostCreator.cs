using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
/*using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Dispatcher;*/

namespace DynJson.Helpers.WebHelpers
{/*
    public static class WcfHostCreator
    {
        public static ServiceHost CreateHostCustom(Type SrvType, String Address, String WsdlAddress, TimeSpan? Timeout)
        {
            Uri url = new Uri(Address);
            ServiceHost host = new ServiceHost(
                SrvType,
                url);
            CustomBinding binding = CreateBindingCustomMax(Timeout);

            host.Description.Endpoints.Clear();
            host.AddServiceEndpoint(
                SrvType,
                binding,
                "");

            Set_DebugBehavior(host);
            Set_MetadataBehavior(host, WsdlAddress);
            Set_MaxItemsInObjectGraph(host);
            Set_ServiceThrottlingBehavior(host);

            return host;
        }

        public static ServiceHost CreateHostTcp(Type SrvType, String Address, String WsdlAddress, TimeSpan? Timeout)
        {
            Uri url = new Uri(Address);
            ServiceHost host = new ServiceHost(
                SrvType,
                url);
            NetTcpBinding binding = CreateBindingTcpMax(Timeout);

            host.Description.Endpoints.Clear();
            host.AddServiceEndpoint(
                SrvType,
                binding,
                "");

            Set_DebugBehavior(host);
            Set_MetadataBehavior(host, WsdlAddress);
            Set_MaxItemsInObjectGraph(host);
            Set_ServiceThrottlingBehavior(host);

            return host;
        }

        public static ServiceHost CreateHostBasic(Type SrvType, String Address, String WsdlAddress, TimeSpan? Timeout)
        {
            Uri url = new Uri(Address);
            ServiceHost host = new ServiceHost(
                SrvType,
                url);
            BasicHttpBinding binding = CreateBindingBasicMax(Timeout);

            host.Description.Endpoints.Clear();
            host.AddServiceEndpoint(
                SrvType,
                binding,
                "");

            Set_DebugBehavior(host);
            Set_MetadataBehavior(host, WsdlAddress);
            Set_MaxItemsInObjectGraph(host);
            Set_ServiceThrottlingBehavior(host);

            return host;
        }

        public static ServiceHost CreateHostPipe(Type SrvType, String Address, String WsdlAddress, TimeSpan? Timeout)
        {
            Uri url = new Uri(Address);
            ServiceHost host = new ServiceHost(
                SrvType,
                url);
            NetNamedPipeBinding binding = CreateBindingPipeMax(Timeout);

            host.Description.Endpoints.Clear();
            host.AddServiceEndpoint(
                SrvType,
                binding,
                "");

            Set_DebugBehavior(host);
            Set_MetadataBehavior(host, WsdlAddress);
            Set_MaxItemsInObjectGraph(host);
            Set_ServiceThrottlingBehavior(host);

            return host;
        }

        public static WebServiceHost CreateHostWeb(Type SrvType, String Address, TimeSpan? Timeout)
        {
            Uri url = new Uri(Address);
            WebServiceHost host = new WebServiceHost(
                SrvType,
                url);
            WebHttpBinding binding = CreateBindingWebMax(Timeout);

            host.Description.Endpoints.Clear();
            host.AddServiceEndpoint(
                SrvType,
                binding,
                "");

            Set_DebugBehavior(host);
            Set_CrossDomainScriptAccess(binding);
            Set_WebHttpBehavior(host);
            Set_MaxItemsInObjectGraph(host);
            Set_ServiceThrottlingBehavior(host);

            return host;
        }

        ////////////////////////////////////////////////

        private static WebHttpBinding CreateBindingWebMax(TimeSpan? Timeout)
        {
            var lBinding = new System.ServiceModel.WebHttpBinding()
            {
                MaxBufferPoolSize = 99999999,
                MaxBufferSize = 99999999,
                MaxReceivedMessageSize = 99999999,
                SendTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                ReceiveTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                OpenTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                CloseTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                TransferMode = System.ServiceModel.TransferMode.Buffered,
            };
            lBinding.ReaderQuotas.MaxArrayLength = 99999999;
            lBinding.ReaderQuotas.MaxBytesPerRead = 99999999;
            lBinding.ReaderQuotas.MaxDepth = 99999999;
            lBinding.ReaderQuotas.MaxNameTableCharCount = 99999999;
            lBinding.ReaderQuotas.MaxStringContentLength = 99999999;
            lBinding.Security.Mode = WebHttpSecurityMode.None;

            return lBinding;
        }

        private static NetTcpBinding CreateBindingTcpMax(TimeSpan? Timeout)
        {
            var lBinding = new System.ServiceModel.NetTcpBinding()
            {
                MaxBufferPoolSize = 99999999,
                MaxBufferSize = 99999999,
                MaxReceivedMessageSize = 99999999,
                SendTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                ReceiveTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                OpenTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                CloseTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                TransferMode = System.ServiceModel.TransferMode.Buffered,
                MaxConnections = 10000,
            };
            lBinding.ReaderQuotas.MaxArrayLength = 99999999;
            lBinding.ReaderQuotas.MaxBytesPerRead = 99999999;
            lBinding.ReaderQuotas.MaxDepth = 99999999;
            lBinding.ReaderQuotas.MaxNameTableCharCount = 99999999;
            lBinding.ReaderQuotas.MaxStringContentLength = 99999999;
            lBinding.Security.Mode = SecurityMode.None;
            lBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.None;
            lBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

            return lBinding;
        }

        private static BasicHttpBinding CreateBindingBasicMax(TimeSpan? Timeout)
        {
            var lBinding = new System.ServiceModel.BasicHttpBinding()
            {
                MaxBufferPoolSize = 99999999,
                MaxBufferSize = 99999999,
                MaxReceivedMessageSize = 99999999,
                SendTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                ReceiveTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                OpenTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                CloseTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                TransferMode = System.ServiceModel.TransferMode.Buffered,
            };
            lBinding.ReaderQuotas.MaxArrayLength = 99999999;
            lBinding.ReaderQuotas.MaxBytesPerRead = 99999999;
            lBinding.ReaderQuotas.MaxDepth = 99999999;
            lBinding.ReaderQuotas.MaxNameTableCharCount = 99999999;
            lBinding.ReaderQuotas.MaxStringContentLength = 99999999;
            lBinding.Security.Mode = BasicHttpSecurityMode.None;

            return lBinding;
        }

        private static CustomBinding CreateBindingCustomMax(TimeSpan? Timeout)
        {
            var lBinaryBinding = new BinaryMessageEncodingBindingElement();
            lBinaryBinding.MaxReadPoolSize = 99999999;
            lBinaryBinding.MaxSessionSize = 99999999;
            lBinaryBinding.MaxWritePoolSize = 99999999;
            lBinaryBinding.ReaderQuotas.MaxArrayLength = 99999999;
            lBinaryBinding.ReaderQuotas.MaxBytesPerRead = 99999999;
            lBinaryBinding.ReaderQuotas.MaxDepth = 99999999;
            lBinaryBinding.ReaderQuotas.MaxNameTableCharCount = 99999999;
            lBinaryBinding.ReaderQuotas.MaxStringContentLength = 99999999;

            var lHttpBindingElement = new HttpTransportBindingElement()
            {
                MaxReceivedMessageSize = 99999999,
                MaxBufferSize = 99999999,
                MaxBufferPoolSize = 99999999,
            };

            var lBinding = new CustomBinding(
                new BindingElement[] {
                    lBinaryBinding,
                    lHttpBindingElement
                });

            lBinding.SendTimeout = Timeout ?? new TimeSpan(0, 0, 30);
            lBinding.ReceiveTimeout = Timeout ?? new TimeSpan(0, 0, 30);
            lBinding.OpenTimeout = Timeout ?? new TimeSpan(0, 0, 30);
            lBinding.CloseTimeout = Timeout ?? new TimeSpan(0, 0, 30);

            return lBinding;
        }

        private static NetNamedPipeBinding CreateBindingPipeMax(TimeSpan? Timeout)
        {
            var lBinding = new System.ServiceModel.NetNamedPipeBinding()
            {
                MaxConnections = 10000,
                MaxBufferPoolSize = 99999999,
                MaxBufferSize = 99999999,
                MaxReceivedMessageSize = 99999999,
                SendTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                ReceiveTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                OpenTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                CloseTimeout = Timeout ?? new TimeSpan(0, 0, 30),
                TransferMode = System.ServiceModel.TransferMode.Buffered,
            };
            lBinding.ReaderQuotas.MaxArrayLength = 99999999;
            lBinding.ReaderQuotas.MaxBytesPerRead = 99999999;
            lBinding.ReaderQuotas.MaxDepth = 99999999;
            lBinding.ReaderQuotas.MaxNameTableCharCount = 99999999;
            lBinding.ReaderQuotas.MaxStringContentLength = 99999999;
            lBinding.Security.Mode = NetNamedPipeSecurityMode.None;

            return lBinding;
        }

        ////////////////////////////////////////////////

        private static void Set_CrossDomainScriptAccess(WebHttpBinding WebHttpBinding)
        {
            WebHttpBinding.CrossDomainScriptAccessEnabled = true;
        }

        private static void Set_DebugBehavior(ServiceHost ServiceHost)
        {
            ServiceDebugBehavior sdb = ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null) { sdb = new ServiceDebugBehavior(); ServiceHost.Description.Behaviors.Add(sdb); }
            sdb.IncludeExceptionDetailInFaults = true;
        }

        private static void Set_MetadataBehavior(ServiceHost ServiceHost, String HttpGetAddress)
        {
            ServiceMetadataBehavior smb = ServiceHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null) { smb = new ServiceMetadataBehavior(); ServiceHost.Description.Behaviors.Add(smb); }
            if (!String.IsNullOrEmpty(HttpGetAddress))
            {
                smb.HttpGetEnabled = true;
                smb.HttpGetUrl = new Uri(HttpGetAddress);
            }
        }

        private static void Set_ServiceThrottlingBehavior(ServiceHost ServiceHost)
        {
            ServiceThrottlingBehavior smb = ServiceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (smb == null) { smb = new ServiceThrottlingBehavior(); ServiceHost.Description.Behaviors.Add(smb); }
            smb.MaxConcurrentCalls = 10000;
            smb.MaxConcurrentInstances = 10000;
            smb.MaxConcurrentSessions = 10000;
        }

        private static void Set_MaxItemsInObjectGraph(ServiceHost ServiceHost)
        {
            // ustawienie DataContractSerializer
            ContractDescription lContract = ServiceHost.Description.Endpoints[0].Contract;
            foreach (OperationDescription lOperationDescription in lContract.Operations)
            {
                // Find the serializer behavior.
                DataContractSerializerOperationBehavior serializerBehavior =
                    lOperationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();

                // If the serializer is not found, create one and add it.
                if (serializerBehavior == null)
                {
                    serializerBehavior = new DataContractSerializerOperationBehavior(lOperationDescription);
                    serializerBehavior.MaxItemsInObjectGraph = 99999999;
                    lOperationDescription.Behaviors.Add(serializerBehavior);
                }

                // Change the settings of the behavior.
                serializerBehavior.MaxItemsInObjectGraph = 99999999;
            }
        }

        private static void Set_WebHttpBehavior(ServiceHost ServiceHost)
        {
            WebHttpBehavior lWebHttpBehavior = ServiceHost.Description.Endpoints.FirstOrDefault().Behaviors.Find<WebHttpBehavior>();

            if (lWebHttpBehavior == null)
            {
                lWebHttpBehavior = new WebHttpBehavior();
                //ServiceHost.Description.Endpoints.FirstOrDefault().Behaviors.Add(lWebHttpBehavior);
                ServiceHost.Description.Endpoints.FirstOrDefault<ServiceEndpoint>().Behaviors.Add(lWebHttpBehavior);
            }

            // CORS
            EnableCrossOriginResourceSharingBehavior lCorsBehavior =
                ServiceHost.Description.Endpoints.FirstOrDefault().Behaviors.Find<EnableCrossOriginResourceSharingBehavior>();

            if (lCorsBehavior == null)
            {
                lCorsBehavior = new EnableCrossOriginResourceSharingBehavior();
                //ServiceHost.Description.Endpoints.FirstOrDefault().Behaviors.Add(lWebHttpBehavior);
                ServiceHost.Description.Endpoints.FirstOrDefault<ServiceEndpoint>().Behaviors.Add(lCorsBehavior);
            }
        }
    }

    public class CustomHeaderMessageInspector : IDispatchMessageInspector
    {
        Dictionary<string, string> requiredHeaders;
        public CustomHeaderMessageInspector(Dictionary<string, string> headers)
        {
            requiredHeaders = headers ?? new Dictionary<string, string>();
        }

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {

            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
            foreach (var item in requiredHeaders)
            {
                httpHeader.Headers.Add(item.Key, item.Value);
            }
        }
    }

    public class EnableCrossOriginResourceSharingBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            var requiredHeaders = new Dictionary<string, string>();

            requiredHeaders.Add("Access-Control-Allow-Origin", "*");
            requiredHeaders.Add("Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS");
            requiredHeaders.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, else-jwt-token, Content-Type, Accept, Authorization");
            requiredHeaders.Add("Access-Control-Allow-Credentials", "true");

            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CustomHeaderMessageInspector(requiredHeaders));
        }

        public void Validate(ServiceEndpoint endpoint)
        {

        }

        public override Type BehaviorType
        {
            get { return typeof(EnableCrossOriginResourceSharingBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new EnableCrossOriginResourceSharingBehavior();
        }
    }*/
}