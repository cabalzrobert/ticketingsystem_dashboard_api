using System;
using System.Collections.Generic;
using System.Linq;
using Comm.Commons.Extensions;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace webapi.App.Features.WebSocketFeature
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true)]
    public class Stomp: Attribute
    {
        public string Destination;
        public Stomp(string Destination){
            this.Destination = Destination;
        }
        /////
        static Stomp(){
            Stomp.Init();
        }
        /////
        public static bool HasMap = false;
        private readonly static ConcurrentDictionary<string, Setting> cachedAutoClass = new ConcurrentDictionary<string, Setting>();
        private readonly static ConcurrentDictionary<string, Setting> cachedMaps = new ConcurrentDictionary<string, Setting>();
        private readonly static ConcurrentDictionary<string, Setting> cachedRegExMaps = new ConcurrentDictionary<string, Setting>();
        
        public static void Init(){
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type target = typeof(StompHandler), targetAuto = typeof(StompHandler.Auto);
            List<Type> types = (from type in assembly.GetTypes()
                where (target.IsAssignableFrom(type) && type.IsClass && type != target) 
                select type).ToList();
            foreach(var type in types)
                cachedAutoClass.TryAdd(type.FullName, new Setting(){ Type = type, AutoClass = targetAuto.IsAssignableFrom(type), });

            var typesWithMethods = (from type in types
                from method in type.GetMethods()
                where (Attribute.IsDefined(method, typeof(Stomp)) 
                    && method.IsPublic && (target.IsAssignableFrom(type) && type != target))
                select (new { Type = type, Method = method, })).ToList();

            if(typesWithMethods.Count != 0){
                Stomp.HasMap = true;
                foreach(var twm in typesWithMethods){
                    twm.Method.GetCustomAttributes(true).ToList().ForEach(a=>{
                        if (a is Stomp) {
                            var mapper = (a as Stomp);
                            var setting = new Setting(){ Destination = mapper.Destination, Type = twm.Type, MethodInfo = twm.Method, };
                            var cached = (!(setting.HasPattern=UriParser.check(mapper.Destination))?cachedMaps:cachedRegExMaps);
                            cached.TryAdd(mapper.Destination, setting);
                        }
                    });
                }
            }
        }
        public class Setting {
            public String Destination;
            public bool HasPattern;
            public Type Type;
            public MethodInfo MethodInfo;
            public bool AutoClass;
            public StompHandler Handler;
            public Stomp.Mapper Mapper;
            public Setting MakeCopy(){
                return new Setting(){
                    Destination = this.Destination,
                    HasPattern = this.HasPattern,
                    Type = this.Type,
                    MethodInfo = this.MethodInfo,
                };
            }
        }
        public class Mapper{
            private ConcurrentDictionary<string, object> cachedInstances = new ConcurrentDictionary<string, object>();
            public WebSocketHandler handler;
            public IServiceScopeFactory scopeFactory;
            public string StompID;
            public Mapper(WebSocketHandler handler, IServiceScopeFactory scopeFactory){
                this.handler = handler;
                this.scopeFactory = scopeFactory;
                this.StompID = DateTime.Now.ToTimeMillisecond().ToString("X").ToLower();
            }
            public void init(){
                this.notify();
                Timeout.Set(initAutoClass, 25);
            }
            private void initAutoClass(){
                List<Setting> list = cachedAutoClass.Where(kvp=>kvp.Value.AutoClass).Select(kvp=>kvp.Value).ToList();
                foreach(Setting setting in list){
                    Setting unlock;
                    lock(setting) unlock = setting.MakeCopy();
                    unlock.Mapper = this;
                    object instance = Instance(unlock);
                }
            }
            public bool invoke(byte[] bytes){
                if(!Stomp.HasMap || cachedInstances==null) return false;
                var data = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                var message = StompMessage.from(data);
                string destination = message.findHeader(StompHeader.DESTINATION);
                if(destination==null)return false;
                Setting setting;
                cachedMaps.TryGetValue(destination, out setting);
                if(setting==null){
                    UriParser up = new UriParser(destination);
                    List<Setting> list = cachedRegExMaps.Select(kvp=>kvp.Value).ToList();
                    string map;
                    foreach(var item in list){
                        lock(item) map = item.Destination;
                        if(up.parser(map)){
                            lock(item) setting = item;
                            break;
                        }
                    }
                }
                if(setting!=null){
                    Setting unlock;
                    lock(setting) unlock = setting.MakeCopy();
                    unlock.Mapper = this;
                    tryInvokeDestination(unlock, message);
                    return true;
                }
                return false;
            }
            
            public void tryInvokeDestination(Setting setting, StompMessage message){
                object instance = Instance(setting);
                if(handler.IsClosed) return;
                try{
                    if(instance is StompHandler.OnMessageReceived)
                        (instance as StompHandler.OnMessageReceived).onMessageReceived();
                    if(instance is StompHandler.OnValidate && !(instance as StompHandler.OnValidate).onValidate())
                        return;
                    Timeout.Set(()=> Invoke(setting, message, instance), 1);
                }catch(Exception e){
                    setting.Handler.stomp(message.addHeader("status", "error").addHeader("message", e.Message));
                }
            }
            private object Instance(Setting setting){
                string cachedID = setting.Type.FullName;
                object instance = null;
                cachedInstances.TryGetValue(cachedID, out instance);
                if(instance==null){
                    instance = Activator.CreateInstance(setting.Type, new object[] { setting });
                    cachedInstances.TryAdd(cachedID, instance);
                }
                setting.Handler = (instance as StompHandler);
                return instance;
            } 
            
            private void Invoke(Setting setting, StompMessage message, object instance){
                try{
                    setting.MethodInfo.Invoke(instance, new object[]{ (instance as StompClient).parse(message) });
                }catch(Exception e){
                    setting.Handler.stomp(message.addHeader("status", "error").addHeader("message", e.Message));
                }
            }
            public void detach(){
                List<object> instances = cachedInstances.Select(kvp=> kvp.Value).ToList();
                foreach(object instance in instances){
                    if(instance is StompHandler.OnValidate && !(instance as StompHandler.OnValidate).onValidate())
                        continue;
                    (instance as StompHandler).closed();
                }
                cachedInstances.Clear();
                //cachedInstances = null;
            }
            //
            public async void notify(){
                await Task.Delay(75);
                await handler.sendString(new StompMessage(StompCommand.CONNECTED, JsonConvert.SerializeObject(new{
                    heartBeat = false,
                    modify = true,
                })).addHeader("session", StompID).compile());
            }
            //
            public T Instance<T>(){
                var type = typeof(T);
                string cachedID = type.FullName;
                object instance = null;
                cachedInstances.TryGetValue(cachedID, out instance);
                if(instance == null){
                    var target = typeof(StompHandler);
                    if(target.IsAssignableFrom(type) && type != target)
                        instance = Activator.CreateInstance(type, new object[] { new Setting{ Type = type, Mapper = this } });
                }
                if(instance==null)
                    instance = default(T);
                return (T)instance;
            }
        }
        public static Mapper over(WebSocketHandler handler, IServiceScopeFactory scopeFactory){
            return new Mapper(handler, scopeFactory);
        }
    }

    public abstract class StompHandler: StompClient {
        protected Stomp.Setting setting;
        protected IServiceScopeFactory scopeFactory;
        public StompHandler(Stomp.Setting setting){
            this.setting = setting;
            this.scopeFactory = setting.Mapper.scopeFactory;
            setWebSocketHandler(setting.Mapper.handler);
        }
        public abstract void closed();
        public interface Auto{}
        public interface OnValidate{
            bool onValidate();
        }
        public interface OnMessageReceived{
            bool onMessageReceived();
        }
        /*
        
        public void ReceivedStompMessage(StompMessage message){}
        */
    }

    public class StompClient
    {   
        private WebSocketHandler handler;
        public void setWebSocketHandler(WebSocketHandler handler){
            this.handler = handler;
        }
        public void stomp(string destination, object data, List<StompHeader> headers = null){
            string subscription_id = null;
            subscriptions.TryGetValue(destination, out subscription_id);
            stomp(new StompMessage(StompCommand.MESSAGE, headers, serialize(data))
                .replaceHeader(StompHeader.DESTINATION, destination)
                .addHeader(StompHeader.SUBSCRIPTION, subscription_id)); //.addHeader(StompHeader.ACK, "client")
        }
        public void stomp(string destination, StompMessage message){
            string subscription_id = null;
            subscriptions.TryGetValue(destination, out subscription_id);
            stomp(message.replaceHeader(StompHeader.DESTINATION, destination)
                .addHeader(StompHeader.SUBSCRIPTION, subscription_id));
        }
        public async void stomp(StompMessage message){
            await handler.sendString(message.compile());
        }
        protected string serialize(object data){
            string message = "";
            if(data != null){
                if(data is String) message = (data as string);
                else message = JsonConvert.SerializeObject(data);
            }
            return message;
        }
        public StompMessage create(object data = null){
            return new StompMessage(StompCommand.MESSAGE, serialize(data));
        }

        private ConcurrentDictionary<string, string> subscriptions = new ConcurrentDictionary<string, string>();
        public StompMessage parse(byte[] bytes){
            return parse(System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length));
        }
        public StompMessage parse(string data){
            return parse(StompMessage.from(data));
        }
        public StompMessage parse(StompMessage message){
            if(handler.IsClosed) return null;
            if(message!=null){
                string command = message.getStompCommand().Str();
                if(command.Equals(StompCommand.SUBSCRIBE)){
                    string DESTINATION = message.findHeader(StompHeader.DESTINATION);
                    if(!DESTINATION.IsEmpty()){
                        string ID = message.findHeader(StompHeader.ID);
                        subscriptions.AddOrUpdate(DESTINATION, ID, (k,v)=>ID);
                    }
                }else if(command.Equals(StompCommand.UNSUBSCRIBE)){
                    string DESTINATION = message.findHeader(StompHeader.DESTINATION);
                    if(!DESTINATION.IsEmpty()){
                        string ID = null;
                        subscriptions.TryRemove(DESTINATION, out ID);
                    }
                }
            }
            return message;
        }
        public void unsubscribes(){
            if(handler.IsClosed) return;
            string ID = null;
            var keys = subscriptions.Keys.ToList();
            foreach(string key in keys)
                subscriptions.TryRemove(key, out ID);
            keys.Clear();
            keys = null;
            ID = null;
        }
    }

    public class StompMessage {

        public readonly static char TERMINATE_MESSAGE_SYMBOL = '\u0000';
        private readonly String mStompCommand;
        private readonly List<StompHeader> mStompHeaders;
        private readonly String mPayload;

        public StompMessage(String stompCommand, List<StompHeader> stompHeaders, String payload) {
            if(stompHeaders==null) stompHeaders = new List<StompHeader>();
            mStompCommand = stompCommand;
            mStompHeaders = stompHeaders;
            mPayload = payload;
        }
        public StompMessage(String stompCommand, String payload) 
            : this(stompCommand, null, payload) {
        }

        public List<StompHeader> getStompHeaders() {
            return mStompHeaders;
        }

        public String getPayload() {
            return mPayload;
        }

        public String getStompCommand() {
            return mStompCommand;
        }

        public String findHeader(String key) {
            if (mStompHeaders == null) return null;
            foreach (StompHeader header in mStompHeaders) {
                if (header.getKey().Equals(key)) return header.getValue();
            }
            return null;
        }
        public StompMessage replaceHeader(string key, string value){
            return replaceHeader(new StompHeader(key, value));
        }
        public StompMessage replaceHeader(StompHeader header){
            var removeHeader = mStompHeaders.Where(h=>h.getKey()==header.getKey()).FirstOrDefault();
            if(removeHeader!=null) mStompHeaders.Remove(removeHeader);
            return addHeader(header);
        }
        public StompMessage addHeader(string key, string value){
            return addHeader(new StompHeader(key, value));
        }
        public StompMessage addHeader(StompHeader header){
            mStompHeaders.Add(header);
            return this;
        }
        public String compile() {
            return compile(false);
        }

        public String compile(bool legacyWhitespace) {
            StringBuilder builder = new StringBuilder();
            builder.Append(mStompCommand).Append('\n');
            foreach (StompHeader header in mStompHeaders) {
                builder.Append(header.getKey()).Append(':').Append(header.getValue()).Append('\n');
            }
            builder.Append('\n');
            if (mPayload != null) {
                builder.Append(mPayload);
                if (legacyWhitespace) builder.Append("\n\n");
            }
            builder.Append(TERMINATE_MESSAGE_SYMBOL);
            return builder.ToString();
        }

        private readonly static Regex PATTERN_COMMAND = new Regex("[^\\s]+");
        private readonly static Regex PATTERN_HEADER = new Regex("([^:\\s]+)\\s*:\\s*([^:\\s]+)");
        private readonly static Regex PATTERN_PAYLOAD = new Regex("\n([^\\s]+)\0$");
        public static StompMessage from(String data) {
            /*if (data == null || data.Trim().IsEmpty()) {
                //return new StompMessage(StompCommand.UNKNOWN, null, data);
                return null;
            }
            Match match;
            match = PATTERN_COMMAND.Match(data);
            if (!match.Success) return null; 
            string command = match.Value;
            match = PATTERN_PAYLOAD.Match(data);
            string payload = (match.Success ? match.Groups[1].Value : null);
            MatchCollection mcHeaders = PATTERN_HEADER.Matches(data);
            List<StompHeader> headers = new List<StompHeader>();
            foreach (Match m in mcHeaders){
                if (m.Groups != null && m.Groups.Count == 3)
                    headers.Add(new StompHeader(m.Groups[1].Value, m.Groups[2].Value));
            }*/
            var reader = new StringReader(data);
            var command = reader.ReadLine();
            var header = reader.ReadLine();
            List<StompHeader> headers = new List<StompHeader>();
            while (!string.IsNullOrEmpty(header))
            {
                var split = header.Split(':');
                if (split.Length == 2) 
                    headers.Add(new StompHeader(split[0].Trim(),split[1].Trim()));
                header = reader.ReadLine() ?? string.Empty;
            }
            var body = reader.ReadToEnd() ?? string.Empty;
            var payload = body.TrimEnd('\r', '\n', '\0');
            return new StompMessage(command, headers, payload);
        }
    }

    public class StompCommand {
        public static readonly String CONNECT = "CONNECT";
        public static readonly String CONNECTED = "CONNECTED";
        public static readonly String SEND = "SEND";
        public static readonly String MESSAGE = "MESSAGE";
        public static readonly String SUBSCRIBE = "SUBSCRIBE";
        public static readonly String UNSUBSCRIBE = "UNSUBSCRIBE";
        public static readonly String UNKNOWN = "UNKNOWN";
    }



    public class StompHeader {

        public readonly static String VERSION = "version";
        public readonly static String HEART_BEAT = "heart-beat";
        public readonly static String DESTINATION = "destination";
        public readonly static String CONTENT_TYPE = "content-type";
        public readonly static String MESSAGE_ID = "message-id";
        public readonly static String ID = "id";
        public readonly static String ACK = "ack";
        public readonly static String SUBSCRIPTION = "subscription";

        private readonly String mKey;
        private readonly String mValue;

        public StompHeader(String key, String value) {
            mKey = key;
            mValue = value;
        }

        public String getKey() {
            return mKey;
        }

        public String getValue() {
            return mValue;
        }
    }
    ////////////////////////////////////
    public class UriParser{
        public string url = "";
        public Dictionary<string, string> result = new Dictionary<string, string>();
        bool isdone = false;
            
        public UriParser(string url){
            this.url = url;
        
        }
        private static System.Text.RegularExpressions.Regex PATTERN = new System.Text.RegularExpressions.Regex(@"{([\w]+)((?<BR>{)|(?<-BR>})|(?(BR){?!})|[^{}]*)+}");
            
        public static bool check(string url){
            return PATTERN.Match(url).Success;
        }

        public bool parser(string urlpattern)
        {
            bool isparse = false;
            var keys = new List<string>();
            var regex = PATTERN;//new System.Text.RegularExpressions.Regex(@"{([\w]+)((?<BR>{)|(?<-BR>})|(?(BR){?!})|[^{}]*)+}");
            var matches = regex.Matches(urlpattern);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                try
                {
                    var find = match.Value.Substring(1, match.Value.Length - 2);
                    var key = match.Groups[1];
                    var pttrn = find.Substring(key.Length); pttrn = (pttrn.Length == 0 ? "[\\w]+" : pttrn);
                    urlpattern = urlpattern.Replace(match.Value, "(?<" + key + ">" + pttrn + ")");
                    keys.Add(key.ToString());
                }
                catch { }
            }
            regex = new System.Text.RegularExpressions.Regex(urlpattern);
            matches = regex.Matches(this.url);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Value == this.url)
                {
                    foreach (var key in keys)
                    {
                        this.result[key] = match.Groups[key].Value;
                    }
                    isparse = true;
                }
                break;
            }
            return isparse;
        }
    }
}