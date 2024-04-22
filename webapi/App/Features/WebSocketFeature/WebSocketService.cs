using System;
using System.Collections.Generic;
using System.Linq;
using webapi.Commons.AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Net.WebSockets;
using System.IO;
using System.Text;
using System.Reflection;
using Comm.Commons.Extensions;

namespace webapi.App.Features.WebSocketFeature
{
    public class WebSocketProvider{
        static WebSocketProvider(){
            WebSocketProvider.Init();
        }
        /////
        private readonly static Dictionary<string, Setting> classes = new Dictionary<string, Setting>();
        public static void Init(){
            Assembly assembly = Assembly.GetExecutingAssembly();
            var target = typeof(WebSocketProvider.On);
            var types = assembly.GetTypes()
                .Where(p => target.IsAssignableFrom(p) && p != target);

            foreach(var type in types)
                classes[type.FullName] = new Setting{ Type=type, Name=type.FullName };
        }
        public class Setting {
            public String Name;
            public Type Type;
            public Object Instance;
        }
        public WebSocketHandler handler;
        public IServiceScopeFactory scopeFactory;

        public WebSocketProvider(WebSocketHandler handler, IServiceScopeFactory scopeFactory){
            this.handler = handler;
            this.scopeFactory = scopeFactory;
        }
        private Dictionary<string, Setting> buildClasses = new Dictionary<string, Setting>();
        public void init(){
            lock(classes) {
                foreach(KeyValuePair<string, Setting> kvp in classes){
                    var setting = kvp.Value;
                    buildClasses[kvp.Key] = new Setting{
                        Name = setting.Name,
                        Type = setting.Type,
                    };
                }
            }
            if(buildClasses.Count != 0){
                foreach(KeyValuePair<string, Setting> kvp in buildClasses){
                    var setting = kvp.Value;
                    setting.Instance = Activator.CreateInstance(setting.Type, new object[] { this });
                    (setting.Instance as On).open();
                }
            }
        }
        public T GetInstance<T>(){
            Setting setting = null;
            if(!handler.IsClosed){
                Type type = typeof(T);
                buildClasses.TryGetValue(type.FullName, out setting);
            }
            object Instance = (setting!=null?setting.Instance:null);
            return (T)(Instance!=null?Instance:null);
        }

        public void detach(){
            if(buildClasses.Count != 0){
                foreach(KeyValuePair<string, Setting> kvp in buildClasses){
                    var setting = kvp.Value;
                    if(setting.Instance!=null){
                        (setting.Instance as On).close();
                        setting.Instance = null;
                    }
                }
                buildClasses.Clear();
            }
            buildClasses = null;
        }
        public interface On {
            void open();
            void close();
        }
    }
    //https://stackoverflow.com/questions/56694527/how-do-i-properly-close-a-persistent-system-net-websockets-clientwebsocket-conne
    //https://stackoverflow.com/questions/23773407/a-websockets-receiveasync-method-does-not-await-the-entire-message
    public class WebSocketHandler: IDisposable {
        public Boolean IsClosed;
        public Boolean IsCrashed;
        public WebSocket webSocket;
        public ControllerBase controller;
        //private On listener;
        //private 
        public WebSocketHandler(ControllerBase controller, WebSocket webSocket){
            this.webSocket = webSocket;
            this.controller = controller;
        }
        private Action<byte[]> _onEmit;
        private Action<WebSocketHandler> _onOpen, _onClose;
        public WebSocketHandler onOpen(Action<WebSocketHandler> func){ _onOpen = func; return this; }
        public WebSocketHandler onClose(Action<WebSocketHandler> func){ _onClose = func;  return this; }
        public WebSocketHandler onEmit(Action<byte[]> func){ _onEmit = func; return this; }
        //
        private WebSocketReceiveResult received;
        private static int MaxReceivedMessageSize = 8192;
        public async Task<bool> start(){
            if(_onOpen != null)  Timeout.Set(()=>_onOpen(this), 1);
            do{
                if(!await receiveByte()) break;
            }while(!IsCrashed && (received==null||!received.CloseStatus.HasValue));
            IsClosed = true;
            if(_onClose != null) _onClose(this);
            if(!IsCrashed){ 
                try{
                    lock(webSocket) 
                        webSocket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
                }catch{}
            }
            return false;
        }

        private async Task<bool> receiveByte(){
            byte[] buffer = new byte[1024 * 4];
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
            try{
                received = await webSocket.ReceiveAsync(segment, CancellationToken.None);
                if(received.MessageType != WebSocketMessageType.Close){
                    byte[] bytes = null;
                    using (var ms = new MemoryStream())
                    {
                        do{
                            ms.Write(segment.Array, segment.Offset, segment.Count);
                            if(MaxReceivedMessageSize<ms.Length){
                                this.forceClose();
                                IsCrashed = true;
                                break;
                            }
                            if(received.EndOfMessage)break;
                            received = await webSocket.ReceiveAsync(segment, CancellationToken.None);
                        }while (true);
                        if(!IsCrashed){
                            ms.Seek(0, SeekOrigin.Begin);
                            bytes = ms.ToArray();
                        }
                    }
                    if(!IsCrashed){
                        if(bytes!=null && bytes.Length>0){
                            _onEmit(bytes);
                            bytes.Clear();
                        }
                    }
                }
            }catch{ IsCrashed = true; }
            segment = null;
            buffer.Clear();
            return true;
        }

        public async Task sendString(string stringContent){
            if(IsClosed)return;
            if(webSocket==null)return;
            var bytes = Encoding.UTF8.GetBytes(stringContent);
            await sendByte(bytes);
            bytes.Clear();
        }
        public async Task<bool> sendByte(byte[] bytes){
            if(IsClosed)return false;
            if(webSocket==null)return false;
            try{
                lock(webSocket) 
                    webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }catch{}
            await Task.Delay(1);
            bytes.Clear();
            return true;
        }
        private async void detach(){
            await Task.Delay(100);
        }
        public void forceClose(){
            try{
                lock(webSocket) 
                    webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "End", CancellationToken.None);
            }catch{}
        }
        //
        bool disposed = false;
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);           
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 
            if (disposing) {
                webSocket = null;
                received = null;
                _onOpen = null;
                _onClose = null;
                _onEmit = null;
            }
            disposed = true;
            //GC.Collect();
        }
    }
}