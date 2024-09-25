using System;
using Comm.Commons.Extensions;
using webapi.Commons.AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
//
using webapi.App.Features.WebSocketFeature;
using webapi.App.Model.User;
using webapi.App.Features.UserFeature;
using Org.BouncyCastle.Asn1.BC;

namespace webapi.App.Aggregates.SubscriberAppAggregate.Features
{
    [Service.ITransient(typeof(SubscriberWs))] 
    public interface ISubscriberWs
    {
        Task Echo(ControllerBase controller, TicketingUser account, WebSocket ws);
    }

    public class SubscriberWs: ISubscriberWs
    {   
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly StompPusher _pusher;
        //
        public SubscriberWs(StompPusher pusher, IServiceScopeFactory serviceScopeFactory){
            _serviceScopeFactory = serviceScopeFactory;
            _pusher = pusher;
        }
        public async Task Echo(ControllerBase controller, TicketingUser account, WebSocket ws){
            using(var handler = new WebSocketHandler(controller, ws)){
                try{
                    var stomp = new SubscriberSc(_pusher, account, handler, _serviceScopeFactory);
                    handler.onEmit(async(bytes)=>{
                        stomp.onMessageReceived(stomp.parse(bytes));
                        await Task.Delay(1);
                    });
                    handler.onClose(async(handler)=>{
                        stomp.Detach();
                        await Task.Delay(1);
                    });
                    await handler.start();
                }catch(Exception e){
                    string str = e.StackTrace;
                }
            }
        }
    }
    public class SubscriberSc: StompClient
    {
        private IServiceScopeFactory scopeFactory;
        private WebSocketHandler handler;
        private TicketingUser account;
        private StompPusher.StackableSubscribe stack;
        public SubscriberSc(StompPusher pusher, TicketingUser account, WebSocketHandler handler, IServiceScopeFactory scopeFactory){
            setWebSocketHandler(this.handler = handler);
            if(account.IsSessionExpired) return;
            this.stack = pusher.create();
            this.account = account;
            this.scopeFactory = scopeFactory;
            //
            this.Subscribes();
        }
        private bool isConnected = false;
        public void onMessageReceived(StompMessage message){
            if(message==null) return;
            string command = message.getStompCommand().Str();
            if(command.Equals(StompCommand.CONNECT)){
                if(isConnected) return;
                isConnected = true;
                stomp(new StompMessage(StompCommand.CONNECTED, message.getStompHeaders(), null)
                    .addHeader("session", ((int)DateTime.Now.ToTimeMillisecond()).ToString("X")));
            }
        }

        private void Subscribes(){
            stack.subscribe("/test", this.msgTest);
            stack.subscribe("/smsin", this.msgSMSIN);
            stack.subscribe($"{account.PL_ID}/{account.PGRP_ID}/emergencyalert", this.receivedBranchEmergencyAlertNotification);
            stack.subscribe($"{account.PL_ID}/{account.PGRP_ID}/issuesandconern", this.receivedBranchIssuesandConcernNotification);
            stack.subscribe($"{account.PL_ID}/{account.PGRP_ID}/requestdocument", this.receivedBranchRequestDocumentNotification);
            stack.subscribe($"{account.PL_ID}/{account.PGRP_ID}/complaint", this.receivedBranchComplaintNotification);
            //stack.subscribe($"/{account.PL_ID}/notify", this.receivedCompanyNotication);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/chat", this.receivedBranchPublicChat);
            //stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/notify", this.receivedNotication);
            stack.subscribe($"/{account.PL_ID}/test/notify", this.receivedNotication);

            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/comment", this.receivedSubscriberCommentTicket);


            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{Convert.ToInt32(account.isCommunicator)}/ticketrequest/iscommunicator", this.receivedTicketRequestCommunicator);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.ACT_TYP}/{account.DEPT_ID}/requestorhead", this.receivedTicketRequestorHead);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.ACT_TYP}/communicator", this.receivedTicketRequestorHead);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/forwardticket", this.receivedFowardTicket);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/requestor", this.receivedTicketRequestor);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.DEPT_ID}/deptforwardticket", this.receivedComForwardTicket);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/return", this.receivedTicketReturned);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/cancel", this.receivedTicketCanceled);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/approval", this.receivedTicketApproval);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/decline", this.receivedTicketDeclined);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/resolve", this.receivedTicketResolved);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/countticket", this.ticketCount);

            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.DEPT_ID}/forwardticket/depthead/{Convert.ToInt32(account.isDeptartmentHead)}", this.receivedTicketForwadedDeptHead);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/assigned", this.receivedTicketAssignedTo);
            //stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/ticketrequest/iscommunicator", this.receivedTicketRequestCommunicator);

            //stack.subscribe($"/{account.CompanyID}/{account.BranchID}/arena", this.receivedBranchArena);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/chat", this.receivedSubscriberChat);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/notify", this.receivedSubscriberNotification);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/balance", this.receivedSubscriberBalance);
            stack.subscribe($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/virtualid", this.receivedSubscriberVirtualID);

            //if(!account.GeneralCoordinatorID.IsEmpty())
            //    stack.subscribe($"/{account.CompanyID}/{account.BranchID}/{account.GeneralCoordinatorID}/downline", this.receivedSubscriberNotification);
            //if(!account.CoordinatorID.IsEmpty() && !account.CoordinatorID.Equals(account.SubscriberID))
            //    stack.subscribe($"/{account.CompanyID}/{account.BranchID}/{account.CoordinatorID}/downline", this.receivedSubscriberNotification);
            //stack.subscribe($"/{account.CompanyID}/{account.BranchID}/{account.GeneralCoordinatorID}/upline", this.receivedSubscriberChat);
            //stack.subscribe($"postbet/notify", (Ultralight.StompMessage message)=>{});
        }
        public void Detach(){
            this.unsubscribes();
            if(stack!=null){
                using(stack){
                    stack.unsubscribes();
                }
            }
        }
        //
        private void msgTest(Ultralight.StompMessage message){
            stomp("/test", message.Body);
        }
        private void msgSMSIN(Ultralight.StompMessage message)
        {
            stomp("/smsin", message.Body);
        }
        private void receivedBranchEmergencyAlertNotification(Ultralight.StompMessage message)
        {
            stomp("/emergencyalert", message.Body);
        }
        private void receivedBranchIssuesandConcernNotification(Ultralight.StompMessage message)
        {
            stomp("/issuesandconern", message.Body);
        }
        private void receivedBranchRequestDocumentNotification(Ultralight.StompMessage message)
        {
            stomp("/requestdocument", message.Body);
        }

        private void receivedBranchComplaintNotification(Ultralight.StompMessage message)
        {
            stomp("/complaint", message.Body);
        }
        private void receivedCompanyNotication(Ultralight.StompMessage message){
            stomp("/company", message.Body);
        }
        private void receivedBranchNotication(Ultralight.StompMessage message){
            stomp("/branch", message.Body);
        }

        private void receivedSubscriberCommentTicket(Ultralight.StompMessage message)
        {
            stomp("/comment", message.Body);
        }
        private void receivedNotication(Ultralight.StompMessage message)
        {
            stomp("/test/notify", message.Body);
        }
        private void receivedTicketRequestCommunicator(Ultralight.StompMessage message)
        {
            stomp("/1/ticketrequest/iscommunicator", message.Body);
        }
        private void receivedTicketRequestorHead(Ultralight.StompMessage message)
        {
            stomp("/requestorhead", message.Body);
        }

        private void receivedTicketRequestor(Ultralight.StompMessage message)
        {
            stomp("/requestor", message.Body);
        }
        private void receivedFowardTicket(Ultralight.StompMessage message)
        {
            stomp("/forwardticket", message.Body);
        }

        private void receivedComForwardTicket(Ultralight.StompMessage message)
        {
            stomp("/deptforwardticket", message.Body);
        }

        private void receivedTicketReturned(Ultralight.StompMessage message)
        {
            stomp("/return", message.Body);
        }

        private void receivedTicketApproval(Ultralight.StompMessage message)
        {
            stomp("/approval", message.Body);
        }

        private void receivedTicketDeclined(Ultralight.StompMessage message)
        {
            stomp("/decline", message.Body);
        }

        private void receivedTicketCanceled(Ultralight.StompMessage message)
        {
            stomp("/cancel", message.Body);
        }

        private void receivedTicketResolved(Ultralight.StompMessage message)
        {
            stomp("/resolve", message.Body);
        }

        private void ticketCount(Ultralight.StompMessage message)
        {
            stomp("/countticket", message.Body);
        }

        private void receivedTicketCommunicator(Ultralight.StompMessage message)
        {
            stomp("/communicator", message.Body);
        }
        private void receivedTicketForwadedDeptHead(Ultralight.StompMessage message)
        {
            stomp("/forwardticket/depthead/1", message.Body);
        }
        private void receivedTicketAssignedTo(Ultralight.StompMessage message)
        {
            stomp("/assigned", message.Body);
        }
        private void receivedBranchArena(Ultralight.StompMessage message){
            stomp("/arena", message.Body);
        }
        private void receivedBranchPublicChat(Ultralight.StompMessage message){
            stomp("/chat/pub", message.Body);
        }
        //
        private void receivedSubscriberChat(Ultralight.StompMessage message){
            stomp("/chat", message.Body);
        }
        
        private void receivedSubscriberVirtualID(Ultralight.StompMessage message)
        {
            stomp("/virtualid", message.Body);
        }
        private void receivedSubscriberNotification(Ultralight.StompMessage message){
            stomp("/notify", message.Body);
        }
        private void receivedSubscriberBalance(Ultralight.StompMessage message){
            stomp("/balance", message.Body);
        }
    }

}