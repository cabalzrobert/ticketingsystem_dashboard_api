using System;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
//
using Newtonsoft.Json;

//
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Comm.Commons.Advance;
using webapi.Commons.AutoRegister;

using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

using System.IO;

using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Net.WebSockets;
//
using System.Reflection;
using webapi.App.Features.WebSocketFeature;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using Ultralight.Client.Transport;
using webapi.App.Aggregates;

namespace webapi.App.Features.UserFeature
{
    [Service.Singleton] 
    public class StompPusher
    {
        private Ultralight.Client.StompClient client;
        private bool isConnected;
        public StompPusher(){
            Timeout.Set(()=>this.StompReceiver());
        }
        private void StompReceiver(){
            client = new Ultralight.Client.StompClient(new WebTransportTransport($"ws://{Pusher.UrlHost}/v1/{Pusher.PublicKey}/ws"));
            client.Connect(this.Connected, this.Reconnect, this.Disconnected);  //client.Connect(this.Connected);
        }
        private void Connected(){
            this.isConnected = true;
            var arr = Subscribe.Subscriptions.Where(kvp=>pingSubscribe(kvp.Value, kvp.Value.destination)).ToArray();
            arr = null;
            Distribute("#connect");
        }
        private void Disconnected(){
            this.isConnected = false;
            Distribute("#disconnect");
            Timeout.Set(this.StompReceiver, 10000);
        }
        private void Reconnect(){
            Distribute("#error");
            /*try{ client.Disconnect(); }catch{}
            client.Dispose();
            Timeout.Set(this.StompReceiver, 10000);*/
        }
        private int iTimeoutPing = 0;
        private void timeoutPing(){
            if(!this.isConnected)return;
            if(iTimeoutPing>0) Timeout.Clear(iTimeoutPing);
            iTimeoutPing = Timeout.Set(()=>this.sendPing(),30000);
        }
        private void sendPing(){
            if(!this.isConnected)return;
            this.client.Send("#ping","");
            this.timeoutPing();
        }

        private bool pingSubscribe(Subscription subscription, string destination, Action<Ultralight.StompMessage> callback = null){
            if(destination.Str().StartsWith('#')){
                if(this.isConnected){
                    if(callback!=null){
                        if(destination=="#connect"){
                            callback(null);
                        }
                    }
                }
                return false;
            }
            this.client.Subscribe(destination, (message)=>distribute(subscription, message));
            return false;
        }

        private void Distribute(string destination, Ultralight.StompMessage message = null){
            if(destination.IsEmpty())return;
            var subscription = Subscribe.Subscription(destination);
            if(subscription==null)return;
            distribute(subscription, message);
        }
        public void distribute(Subscription subscription, Ultralight.StompMessage message = null){
            subscription.distribute(message);
            this.timeoutPing();
        }

        public Subscribe subscribe(string destination, Action<Ultralight.StompMessage> callback){
            if(destination.IsEmpty()) return null;
            return new Subscribe(this, destination, callback);
        }
        public StackableSubscribe create(){
            return new StackableSubscribe(this);
        }

        public class Subscribe : IDisposable 
        {
            public static readonly ConcurrentDictionary<string, Subscription> Subscriptions = new ConcurrentDictionary<string, Subscription>();
            private static readonly Object locker = new Object();
            //
            private long id = 0;
            private Subscription subscription;
            public Subscribe(StompPusher stomp, string destination, Action<Ultralight.StompMessage> callback){
                Subscription subscription = Subscription(destination);
                bool hasSubscription = true;
                if(subscription==null){
                    lock(locker){
                        subscription = Subscription(destination);
                        if(subscription==null){
                            subscription = new Subscription(destination);
                            Subscriptions.AddOrUpdate(destination, subscription, (n,u)=>subscription);
                            hasSubscription = false;
                        }
                    }
                }
                this.id = subscription.add(callback);
                this.subscription = subscription;
                if(!hasSubscription) stomp.pingSubscribe(subscription, destination, callback);
            }
            public bool Unsubscribe(){
                if(subscription==null || id==0) return false;
                subscription.remove(id);
                return false;
            }
            //
            bool disposed = false;
            public void Dispose(){ 
                Dispose(true);
                GC.SuppressFinalize(this);    
            }
            protected virtual void Dispose(bool disposing){
                if (disposed)
                    return; 
                if (disposing) {
                    id = 0;
                    subscription = null;
                }
                disposed = true;
                //GC.Collect();
            }
            //
            public static Subscription Subscription(string destination){
                Subscription subscription = null;
                Subscriptions.TryGetValue(destination, out subscription);
                return subscription;
            }
        }

        public class StackableSubscribe: IDisposable 
        {
            private StompPusher singleton;
            private List<Subscribe> subscribes;
            public StackableSubscribe(StompPusher singleton){
                this.singleton = singleton;
                this.subscribes = new List<Subscribe>();
            }
            public void subscribe(string destination, Action<Ultralight.StompMessage> callback){
                subscribes.Add(singleton.subscribe(destination, callback));
            }
            public void unsubscribes(){
                if(subscribes==null)return;
                var arr = subscribes.Where(sub=>sub.Unsubscribe()).ToArray();
                arr = null;
            }
            //
            bool disposed = false;
            public void Dispose(){ 
                Dispose(true);
                GC.SuppressFinalize(this);    
            }
            protected virtual void Dispose(bool disposing){
                if (disposed)
                    return; 
                if (disposing) {
                    if(subscribes!=null){
                        var arr = subscribes.Where(Dispose).ToArray();
                        arr = null;
                        subscribes.Clear();
                    }
                    subscribes = null;
                    singleton = null;
                }
                disposed = true;
                //GC.Collect();
            }
            private static bool Dispose(Subscribe subscribe){
                subscribe.Dispose();
                return false;
            }
        }

        public class Subscription
        {
            public string destination;
            private long counterID = 0;
            private ConcurrentDictionary<long, Action<Ultralight.StompMessage>> callbacks;
            public Subscription(string destination){
                this.destination = destination;
                this.callbacks = new ConcurrentDictionary<long, Action<Ultralight.StompMessage>>();
            }
            public long add(Action<Ultralight.StompMessage> callback){
                long id = ++this.counterID;
                callbacks.TryAdd(id, callback);
                return id;
            }
            public bool remove(long id){
                Action<Ultralight.StompMessage> callback = null;
                callbacks.TryRemove(id, out callback);
                callback = null;
                return false;
            }
            public void distribute(Ultralight.StompMessage message = null){
                var arr = callbacks.Where(kvp=>perform(kvp.Value, message)).ToArray();
                arr = null;
            }
            private bool perform(Action<Ultralight.StompMessage> action, Ultralight.StompMessage message){
                action(message);
                return false;
            }
        }
    }
}