using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SimpleBox.Models;
using SimpleBox.Utils;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SimpleBox.Core
{
    public sealed class WebPush
    {
        #region Current

        public static WebPush Current;

        #endregion

        #region Core Data

        private WebSocketServer _server;

        #endregion

        #region Temporary Data

        private JObject _lastMessage = new JObject
        {
            ["type"] = "heart"
        };

        #endregion

        #region Constructor

        public WebPush()
        {
            _server = new WebSocketServer(19101);
            _server.AddWebSocketService<MallowWebSocketBehavior>("/");
        }

        #endregion

        #region Life Cycle Methods

        public void Start() => _server.Start();

        public void Stop() => _server.Stop();

        public bool IsListening => _server.IsListening;

        #endregion

        #region Core Methods

        private void Push(JObject jObject, Action completed)
        {
            _lastMessage = jObject;
            _server.WebSocketServices.BroadcastAsync(_lastMessage.ToString(), completed);
        }

        #endregion

        #region Push Methods

        public void RePush(Action completed) => Push(_lastMessage, completed);

        public void PushConfig()
        {

        }

        public void PushMallow(Mallow mallow, Action completed)
        {
            if (mallow is null) return;
            Push(new JObject
                {
                    ["type"] = "msg",
                    ["data"] =
                    {
                        ["msg"] = mallow.LocalMessage,
                        ["response"] = mallow.LocalResponse,
                        ["createTime"] = mallow.LocalCreateTime is null
                            ? 0
                            : DateTimeUtils.ConvertDateToJsTicks((DateTime) mallow.LocalCreateTime),
                        ["responseTime"] = mallow.LocalResponseTime is null
                            ? 0
                            : DateTimeUtils.ConvertDateToJsTicks((DateTime) mallow.LocalResponseTime)
                    }
                },
                completed);
        }

        #endregion
    }

    internal class MallowWebSocketBehavior : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            if (WebPush.Current is null) return;
            switch (e.Data)
            {
                case "heart":
                    WebPush.Current.RePush(() => { });
                    break;
            }
        }
    }
}
