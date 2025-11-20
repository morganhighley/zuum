// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="RokuPremiereTcpProtocol.cs">
//   
// </copyright>
// 
// --------------------------------------------------------------------------------------------------------------------

using Crestron.RAD.Common;
using Crestron.SimplSharp;

using System;
using System.Text;
using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp.Net.Http;

namespace Crestron.RAD.Drivers.VideoServers
{
    using Crestron.RAD.Common.BasicDriver;
    using Crestron.RAD.Common.Enums;
    using Crestron.RAD.Common.Transports;
    using Crestron.RAD.DeviceTypes.VideoServer;
    using Crestron.RAD.Common.ExtensionMethods;

    public class RokuPremiereProtocol : AVideoServerProtocol
    {
        internal bool MediaCommandJustGiven = false;
        private AbsoluteValidator _responseValidatorRef;
        private RokuPremiereTcp _device;



        public RokuPremiereProtocol(ISerialTransport transportDriver, byte id, RokuPremiereTcp Device)
            : base(transportDriver, id)
        {
            Id = id;
            _responseValidatorRef = new AbsoluteValidator(id, ValidatedData, this);
            ResponseValidation = _responseValidatorRef;
            _device = Device;
        }


        public override void SelectMediaService(string mediaServiceId)
        {
            if (!string.IsNullOrEmpty(mediaServiceId))
            {
                base.SelectMediaService(mediaServiceId);
                MediaCommandJustGiven = true;

                //Allow for Event to go up rather than wait for response.
                //Some Apps take up to 10sec to load.
                var mediaServiceData = VideoServerData.CrestronSerialDeviceApi.Api.MediaServiceProviders;
                if (mediaServiceData.Exists() &&
                        mediaServiceData.FeedbackData.Exists() &&
                        mediaServiceData.FeedbackData.ActiveServiceFeedbackData.Exists())
                {
                    var activeServiceFeedback = mediaServiceData.FeedbackData.ActiveServiceFeedbackData.Feedback;
                    if (activeServiceFeedback.Exists())
                    {
                        if (activeServiceFeedback.ContainsKey(mediaServiceId))
                        {
                            string response = activeServiceFeedback[mediaServiceId];
                            //CrestronConsole.PrintLine("In override SelectMediaService found response:{0}", response);
                            DeConstructActiveMediaServiceFeedback(response);
                        }
                        else if (EnableLogging)
                        {
                            Log(string.Format("Unable to select media service - invalid ID provided:{0}", mediaServiceId));
                        }
                    }
                }
            }
            else if (EnableLogging)
            {
                Log("Unable to select media service - no ID provided");
            }
        }

        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            //CrestronConsole.PrintLine("In 1 PrepareStringthenSend command:{0}:  group:{1}:  name:{2}: IsConnected:{3} pollSent:{4}", commandSet.Command, commandSet.CommandGroup, commandSet.CommandName, IsConnected, _device.PollSent);

            if ((commandSet.CommandGroup == CommonCommandGroupType.MediaService) && (commandSet.Command.Equals("poll")))
            {
                if (_device.PollSent == false)
                {
                    return true;
                }

                commandSet.Command = "query/active-app";
            }

            if (commandSet.CommandPrepared)
            {
                return base.PrepareStringThenSend(commandSet);
            }

            return base.PrepareStringThenSend(commandSet);
        }

        public override void Search()
        {
            CommandSet command = new CommandSet("Search", "search/browse", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.Search);
            SendCommand(command);
        }

        public override void PlayPause()
        {
            CommandSet command = new CommandSet("PlayPause", "keypress/play", CommonCommandGroupType.Other, null, false, CommandPriority.Normal, StandardCommandsEnum.PlayPause);
            SendCommand(command);
        }


        public override void DeConstructActiveMediaServiceFeedback(string response)
        {
            //CrestronConsole.PrintLine("In override DeConstructActiveMediaServiceFeedback:{0}", response);
            base.DeConstructActiveMediaServiceFeedback(response);
        }

        /********
        protected override void ConnectionChanged(bool connection)
        {
            //CrestronConsole.PrintLine("In override ConnectionChanged connection:{0} IsConnected:{1}",connection, IsConnected);
            base.ConnectionChanged(connection);
        }
         * ******/


    }

    //public class DelimiterValidator : ResponseValidation
    public class AbsoluteValidator : ResponseValidation
    {
        private RokuPremiereProtocol _protocol;


        //public DelimiterValidator(byte id, DataValidation dataValidation)
        //    : base(id, dataValidation)
        public AbsoluteValidator(byte id, DataValidation dataValidation, RokuPremiereProtocol protocol)
            : base(id, dataValidation)
        {
            Id = id;
            DataValidation = dataValidation;
            _protocol = protocol;
        }

        public override ValidatedRxData ValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            var validatedRxData = new ValidatedRxData(false, null);

            //CrestronConsole.PrintLine("In ValidateResponse MediaCommandJustgiven:{0} ResponseRcvd:{1}:", _protocol.MediaCommandJustGiven, response);

            if (response.Contains("\r\n\r\n") &&
                response.Contains(DataValidation.AckDefinition))
            {
                //Make sure string indicates it is valid xml
                if (response.Contains("<?xml version="))
                {
                    //Determine if this is response to active-app query or not
                    if (response.Contains("<active-app>"))
                    {
                        //Exclude screensaver responses
                        if (response.Contains("<screensaver id="))
                        {
                            _protocol.MediaCommandJustGiven = false;
                            validatedRxData.Data = "unknown";
                            validatedRxData.Ready = true;
                            validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                        }
                        else if ((response.Contains("app id=")) && (_protocol.MediaCommandJustGiven == false))
                        {
                            //Find position of "app id="
                            int startOfAppID = response.IndexOf("app id=");
                            if (startOfAppID > -1)
                            {
                                startOfAppID++;
                                int endOfAppID = response.IndexOf('>', startOfAppID);
                                if (endOfAppID > -1)
                                {
                                    int startOfAppName = endOfAppID + 1;
                                    int endOfAppName = response.IndexOf("</app", startOfAppName);
                                    if (endOfAppName > -1)
                                    {
                                        string appName = response.Substring(startOfAppName,
                                                           (endOfAppName - startOfAppName));

                                        //CrestronConsole.PrintLine("Appname found:{0}:", appName);
                                        validatedRxData.Data = appName;
                                        validatedRxData.Ready = true;
                                        validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                                    }
                                    else
                                    {
                                        validatedRxData.Data = "unknown";
                                        validatedRxData.Ready = true;
                                        validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                                    }
                                }
                                else
                                {
                                    validatedRxData.Data = "unknown";
                                    validatedRxData.Ready = true;
                                    validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                                }
                            }
                            else
                            {
                                validatedRxData.Data = "unknown";
                                validatedRxData.Ready = true;
                                validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                            }

                        }
                        else if ((response.Contains("app id=")) && (_protocol.MediaCommandJustGiven == true))
                        {
                            _protocol.MediaCommandJustGiven = false;
                            validatedRxData.Data = response;
                            validatedRxData.Ignore = true;
                        }
                        else
                        {
                            _protocol.MediaCommandJustGiven = false;
                            validatedRxData.Data = "unknown";
                            validatedRxData.Ready = true;
                            validatedRxData.CommandGroup = CommonCommandGroupType.MediaService;
                        }
                    }
                    else
                    {
                        _protocol.MediaCommandJustGiven = false;
                        validatedRxData.Data = "HTTP/1.1 200 OK";
                        validatedRxData.Ready = true;
                        validatedRxData.CommandGroup = CommonCommandGroupType.AckNak;
                    }
                }
                else
                {
                    _protocol.MediaCommandJustGiven = false;
                    validatedRxData.Data = "HTTP/1.1 200 OK";
                    validatedRxData.Ready = true;
                    validatedRxData.CommandGroup = CommonCommandGroupType.AckNak;
                }
            }
            else
            {
                _protocol.MediaCommandJustGiven = false;
                validatedRxData.Data = DataValidation.NakDefinition;
                validatedRxData.Ready = true;
                validatedRxData.CommandGroup = CommonCommandGroupType.AckNak;
            }
            return validatedRxData;
        }
    }




    public class RokuPremiereTcpHttpTransport : ATransportDriver
    {
        private string _ipAddress;
        private int _port;
        private HttpClient _client;
        private RokuPremiereTcp _device;

        public bool Connected { get; private set; }

        public RokuPremiereTcpHttpTransport(RokuPremiereTcp Device)
        {
            IsEthernetTransport = true;
            _client = new HttpClient();
            _device = Device;
        }

        public void Initialize(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress.ToString();
            _port = port;
        }

        public override void Start()
        {
            if (_client == null)
            {
                _client = new HttpClient();
            }
        }

        public override void Stop()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            HttpClientRequest request = new HttpClientRequest();
            RequestType requestType;
            if (message.Contains("query"))
            {
                requestType = RequestType.Get;
            }
            else
            {
                requestType = RequestType.Post;
            }

            if (paramaters != null)
            {
                foreach (object obj in paramaters)
                {
                    if (obj.GetType() == typeof(RequestType))
                    {
                        requestType = (RequestType)obj;
                        break;
                    }
                }
            }

            request.Url.Parse(String.Format("http://{0}:{1}/{2}", _ipAddress, _port, message));
            request.RequestType = requestType;
            request.KeepAlive = false;

            if (EnableTxDebug)
            {
                StringBuilder debugStringBuilder = new StringBuilder("TX: ");
                debugStringBuilder.Append(request.Url);
                debugStringBuilder.Append('\n');
                debugStringBuilder.Append(request.Header);
                debugStringBuilder.Append(request.ContentString);
                debugStringBuilder.Append('\n');
                Log(debugStringBuilder.ToString());
            }
            //if (request.Header != null)
            //    CrestronConsole.PrintLine("In SendMethod request.NumHeaders:{0}", request.Header.Count);
            //else
            //    CrestronConsole.PrintLine("In SendMethod header count is 0");

            var responseCode = _client.DispatchAsync(request, HostResponseCallback);
            if (responseCode != HttpClient.DISPATCHASYNC_ERROR.PENDING &&
                responseCode != HttpClient.DISPATCHASYNC_ERROR.PROCESS_BUSY)
            {
                HandleConnectionStatus(false);
            }
        }

        public void HostResponseCallback(HttpClientResponse response, HTTP_CALLBACK_ERROR Error)
        {
            //CrestronConsole.PrintLine("In HostResponseCallBack Error:{0}", Error);
            try
            {
                switch (Error)
                {
                    case HTTP_CALLBACK_ERROR.COMPLETED:
                        HandleConnectionStatus(true);
                        break;
                    case HTTP_CALLBACK_ERROR.INVALID_PARAM:
                    case HTTP_CALLBACK_ERROR.UNKNOWN_ERROR:
                        HandleConnectionStatus(false);
                        break;
                }
                if (response != null &&
                    DataHandler != null)
                {
                    if (EnableRxDebug)
                    {
                        StringBuilder debugStringBuilder = new StringBuilder("RX: ");
                        debugStringBuilder.Append(response.Header);
                        debugStringBuilder.Append(response.ContentString);
                        debugStringBuilder.Append('\n');
                        Log(debugStringBuilder.ToString());
                    }
                    DataHandler(response.Header + response.ContentString);
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Exception while handling HostResponseCallback: {0}", e.Message));
                }
            }
        }

        private void HandleConnectionStatus(bool isConnected)
        {
            //CrestronConsole.PrintLine("In HandleConnectionStatus isConnected:{0} Connected:{1}", isConnected,Connected);
            if (isConnected)
            {
                if (!_device.Connected)
                {
                    if (ConnectionChanged != null)
                    {
                        //CrestronConsole.PrintLine("In HandleConnectionStatus calling ConnectionChanged");
                        ConnectionChanged(true);
                    }
                }
            }
            else
            {
                if (_device.Connected)
                {
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(false);
                    }
                }
            }
            Connected = _device.Connected;
        }
    }
}
