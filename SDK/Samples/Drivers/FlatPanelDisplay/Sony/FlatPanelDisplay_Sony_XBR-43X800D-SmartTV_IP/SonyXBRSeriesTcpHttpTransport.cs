// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Transports;
using Crestron.SimplSharp.Net.Http;
using Crestron.RAD.Drivers.Displays;
using System.Text.RegularExpressions;


namespace Crestron.RAD.Drivers.Displays
{

    public class SonyXBRSeriesTcpHttpTransport : ATransportDriver
    {

        private string _ipAddress;
        private int _port;
        private HttpClient _client;
        private SonyXBRSeriesTcpProtocol _protocol;

        public bool Connected { get; private set; }

        public SonyXBRSeriesTcpHttpTransport()
        {
            IsEthernetTransport = true;
            _client = new HttpClient();
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
        public void SetProtocol(ref SonyXBRSeriesTcpProtocol Protocol)
        {
            _protocol = Protocol;
        }

        public override void SendMethod(string message, object[] paramaters)
        {
            if (string.IsNullOrEmpty(_protocol.PSK))
            {
                if (EnableLogging)
                {
                    Log("Please enter Pre-Shared Key");
                }
            }

            if (!String.IsNullOrEmpty(message) && !string.IsNullOrEmpty(_protocol.PSK))
            {
                string PSK = _protocol.PSK;                 //Pre shared key to insert into header
                string serviceName = string.Empty;
                try
                {
                    var splitMessage = Regex.Split(message, "@@@@");
                    serviceName = splitMessage[0];
                    message = splitMessage[1];

                }
                catch (Exception e)
                {
                    Log("Error parsing message in the transport: " + e.Message);
                }
                HttpClientRequest request = new HttpClientRequest();

                if (!String.IsNullOrEmpty(message))
                {
                    //Adds Message body
                    request.ContentString = message;
                }

                RequestType requestType = RequestType.Post;     //all comands are sent via Post

                if (serviceName.Equals("IRCC"))                 //Headers for IRCC commands.
                {
                    request.Header.ContentType = "text/xml; charset=UTF-8";
                    request.Header.AddHeader(new HttpHeader("SOAPACTION", "\"urn:schemas-sony-com:service:IRCC:1#X_SendIRCC\""));
                }
                else    //headers for all other commands
                {
                    request.Header.ContentType = "application/json; charset=UTF-8";
                }

                request.Header.AddHeader(new HttpHeader("Content-Length", message.Length.ToString()));

                //Authentication Key Header
                request.Header.AddHeader(new HttpHeader("X-Auth-PSK", PSK));

                request.Url.Parse(String.Format("http://{0}:{1}/sony/{2}", _ipAddress, _port, serviceName));
                request.RequestType = requestType;
                request.KeepAlive = false;

                if (EnableTxDebug)
                {
                    StringBuilder debugStringBuilder = new StringBuilder("TX: ");
                    debugStringBuilder.Append(request.Url);
                    debugStringBuilder.Append('\n');
                    debugStringBuilder.Append(request.Header);
                    debugStringBuilder.Append('\n');
                    debugStringBuilder.Append(request.ContentString);
                    debugStringBuilder.Append('\n');
                    Log(debugStringBuilder.ToString());
                }

                var responseCode = _client.DispatchAsync(request, HostResponseCallback);

                if (responseCode != HttpClient.DISPATCHASYNC_ERROR.PENDING &&
                    responseCode != HttpClient.DISPATCHASYNC_ERROR.PROCESS_BUSY)
                {
                    HandleConnectionStatus(false);
                }
            }
        }

        public void HostResponseCallback(HttpClientResponse response, HTTP_CALLBACK_ERROR Error)
        {
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
                        debugStringBuilder.Append('\n');
                        debugStringBuilder.Append(response.ContentString);
                        debugStringBuilder.Append('\n');
                        Log(debugStringBuilder.ToString());
                    }
                    DataHandler(response.Header + response.ContentString);
                }
            }
            catch (Exception e)
            {

                Log(string.Format("Error Found in HostResponseCallback: {0}", e.Message));
            }
        }

        private void HandleConnectionStatus(bool isConnected)
        {
            if (isConnected)
            {
                if (!Connected)
                {
                    Connected = true;
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(Connected);
                    }
                }
            }
            else
            {
                if (Connected)
                {
                    Connected = false;
                    if (ConnectionChanged != null)
                    {
                        ConnectionChanged(Connected);
                    }
                }
            }
        }
    }

}