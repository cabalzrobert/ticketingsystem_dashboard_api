using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.SubscriberApp;
using webapi.App.RequestModel.SubscriberApp.Common;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using System.IO;
using System.Net;

using Comm.Commons.Advance;
using webapi.App.RequestModel.Feature;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.Services.Firebase;
using webapi.App.Features.UserFeature;
using webapi.App.RequestModel.Common;
using webapi.App.Aggregates.Common.Dto;

namespace webapi.App.Aggregates.FeatureAggregate
{
    [Service.ITransient(typeof(MessengerAppRepository))] 
    public interface IMessengerAppRepository
    {
        Task<(Results result, object item)> RequestPublicChatAsync();
        Task<(Results result, object item)> RequestPersonalChatAsync(String RequestID);
        Task<(Results result, object item)> GetPreviousChatAsync(String ChatKey, int StartWith);
        Task<(Results result, object item)> SendMessageAsync(String ChatKey, MessengerAppRequest request);
        Task<(Results result, object items)> GetPreviousChatsAsync(String LastChatTimestamp);
        Task<(Results result, object items)> GetRecentChatsAsync();
        Task<(Results result, object sender)> Load_ChatSender(FilterRequest req);
    }

    public class MessengerAppRepository : IMessengerAppRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } }  
        public MessengerAppRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }

        public async Task<(Results result, object item)> RequestPublicChatAsync(){
            String SenderID = $"{ account.PL_ID }{ account.ACT_ID }";
            String ChatKey = Cipher.MD5Hash($"{ account.PL_ID }|{ account.PGRP_ID }");
            var results = _repo.DQueryMultiple($@"
                DECLARE @ChatID bigint;
                DECLARE @CurrentDT datetime = GETDATE();

                SELECT TOP(1) @ChatID=CHT_ID FROM dbo.STL0BA with(nolock) WHERE CHT_CD=@ChatKey;
                IF(ISNULL(@ChatID,'')='')
                BEGIN
                    -- additional                
                    DECLARE @CompanyName varchar(250);
                    SELECT TOP(1) @CompanyName=PL_NM, @CompanyID=PL_ID
                    FROM dbo.STLCAA with(nolock) WHERE PL_ID=@CompanyID
                    ORDER BY PL_ID DESC;
                    -- 
                    INSERT INTO dbo.STL0BA(CHT_CD, S_GRP, S_PBLC)
                    VALUEs(@ChatKey, 1, 1);
                    SET @ChatID = SCOPE_IDENTITY();
                    --
                    INSERT INTO dbo.STL0BB(CHT_ID, MMBR_ID, S_ADMN, FLL_NM, DSPLY_NM) 
                    VALUES(@ChatID, '0000000000000000', 1, @CompanyName, @CompanyName);
                END;
                --
                IF(NOT EXISTS(SELECT TOP(1)1 FROM dbo.STL0BB with(nolock) WHERE CHT_ID=@ChatID AND MMBR_ID=@SenderID))
                BEGIN
                    -- additional    
                    DECLARE @Fullname varchar(150), @DisplayName varchar(150), @ProfileImageUrl varchar(500);
                    (SELECT TOP(1) @DisplayName=NCK_NM, @ProfileImageUrl=IMG_URL, @Fullname=FLL_NM
                    FROM dbo.STLBDB a with(nolock) 
                    WHERE CONCAT(a.PL_ID, a.ACT_ID)=@SenderID) ORDER BY a.PL_ID, a.USR_ID;
                    IF(ISNULL(@DisplayName,'')='') SET @DisplayName=@Fullname;
                    --
                    INSERT INTO dbo.STL0BB(CHT_ID, MMBR_ID, FLL_NM, DSPLY_NM) --, ProfileImageUrl
                    VALUES(@ChatID, @SenderID, @Fullname, @DisplayName); 
                END;
                --
                DECLARE @RowCount int;
                DECLARE @tblConversation table(
                    ID bigint, ChatID bigint
                    ,SenderID varchar(25), DisplayName varchar(100), ProfileImageUrl varchar(500)
                    ,Message nvarchar(500), DateSend datetime
                    ,IsFirstMessage bit
                    ,IsImage bit, IsFile bit, MediaUrl varchar(500)
                    ,IsYou bit
                );
                INSERT INTO @tblConversation(ChatID, ID, SenderID, DisplayName, ProfileImageUrl, Message, DateSend, IsFirstMessage
                    , IsImage, IsFile, MediaUrl, IsYou)
                SELECT DISTINCT TOP(15) @ChatID, CHT_CONV_ID, SNDR_ID, DSPLY_NM, PROF_IMG_URL, MSG, SND_TS, S_FRST_MSG
                    , S_IMG, S_FIL, MDIA_URL, CAST(IIF(@SenderID=SNDR_ID, 1,0) as bit)
                FROM dbo.STL0BC with(nolock)
                WHERE CHT_ID=@ChatID AND (SND_TS<@CurrentDT) 
                ORDER BY SND_TS DESC;
                --
                                
                SELECT a.CHT_ID ID, a.CHT_CD ChatKey, a.S_PRSNL IsPersonal, a.S_GRP IsGroup, a.S_PBLC IsPublic, a.S_ALLW_INVT IsAllowInvatiation
                    , b.MMBR_ID MemberID, b.S_ADMN IsAdmin, b.DSPLY_NM DisplayName, b.FLL_NM Fullname, b.PROF_IMG_URL ProfileImageUrl
                FROM dbo.STL0BA a with(nolock)
                INNER JOIN dbo.STL0BB b with(nolock)ON a.CHT_ID=b.CHT_ID 
                WHERE a.CHT_ID=@ChatID AND ((a.S_PRSNL=1 AND b.MMBR_ID<>@SenderID) OR (a.S_GRP=1 AND b.MMBR_ID='0000000000000000'));
                --
                SELECT * FROM @tblConversation;
                --DELETE GARBAGE
                DELETE FROM @tblConversation;
            ", new Dictionary<string, object>(){
                { "ChatKey", ChatKey },
                { "SenderID", SenderID },
                { "CompanyID", account.PL_ID },
            });
            if(results != null){
                var chat = results.ReadSingle<dynamic>();
                if(chat!=null){
                    chat.Messages = results.Read<dynamic>();
                    return (Results.Success, chat);
                }
            }
            return (Results.Null, null);
        }


        public async Task<(Results result, object item)> RequestPersonalChatAsync(String RequestID){
            String SenderID = $"{ account.PL_ID }{ account.ACT_ID }";
            String ReceiverID = $"{ account.PL_ID }{ RequestID }";
            String strChatKey = DateTime.Now.ToTimeMillisecond().ToString("X");
            var results = _repo.DQueryMultiple($@"
                DECLARE @ChatID bigint;
                DECLARE @CurrentDT datetime = GETDATE();
                ;with cte as(
                    SELECT CHT_ID ChatID, MMBR_ID MemberID
                    FROM dbo.STL0BB with(nolock)
                    WHERE MMBR_ID IN (@SenderID, @ReceiverID)
                    AND CHT_ID IS NOT NULL
                )
                SELECT TOP(1) @ChatID=CHT_ID
                FROM (SELECT DISTINCT a.ChatID 
                    FROM(SELECT DISTINCT ChatID, COUNT(0)Count FROM cte with(nolock) GROUP BY ChatID) a
                    INNER JOIN cte b with(nolock) ON (a.ChatID=b.ChatID)
                    INNER JOIN cte c with(nolock) ON (b.ChatID=c.ChatID)
                    WHERE a.Count>1
                    AND (b.MemberID=@SenderID AND c.MemberID=@ReceiverID)) a
                INNER JOIN dbo.STL0BA b with(nolock) ON a.ChatID=b.CHT_ID
                WHERE b.S_PRSNL=1;
                --
                IF(ISNULL(@ChatID,'')='')
                BEGIN
                    INSERT INTO dbo.STL0BA(CHT_CD, S_PRSNL)
                    VALUEs(@ChatKey, 1);
                    SET @ChatID = SCOPE_IDENTITY();
                    INSERT INTO dbo.STL0BB(CHT_ID, MMBR_ID, FLL_NM)
                    VALUES(@ChatID, @SenderID, @SenderID);
                    INSERT INTO dbo.STL0BB(CHT_ID, MMBR_ID, FLL_NM) 
                    VALUES(@ChatID, @ReceiverID, @ReceiverID); 
                END;
                --
                SELECT a.CHT_ID ID, a.CHT_CD ChatKey, a.S_PRSNL IsPersonal, a.S_GRP IsGroup, a.S_PBLC IsPublic, a.S_ALLW_INVT IsAllowInvatiation
                    , b.MMBR_ID MemberID, b.S_ADMN IsAdmin, b.DSPLY_NM DisplayName, b.FLL_NM Fullname, b.PROF_IMG_URL ProfileImageUrl
                FROM dbo.STL0BA a with(nolock)
                INNER JOIN dbo.STL0BB b with(nolock)ON a.CHT_ID=b.CHT_ID 
                WHERE a.CHT_ID=@ChatID AND ((a.S_PRSNL=1 AND b.MMBR_ID<>@SenderID) OR (a.S_GRP=1 AND b.MMBR_ID='0000000000000000'));
                --
                SELECT DISTINCT TOP(15) a.CHT_ID ChatID, a.CHT_CONV_ID ID, a.SNDR_ID SenderID, a.DSPLY_NM DisplayName, a.PROF_IMG_URL ProfileImageUrl
                    , a.MSG Message, a.SND_TS DateSend, a.S_FRST_MSG IsFirstMessage
                    , a.S_IMG IsImage, a.S_FIL IsFile, a.MDIA_URL MediaUrl, CAST(IIF(@SenderID=a.SNDR_ID,1,0) as bit) IsYou
                FROM dbo.STL0BC a with(nolock)
                INNER JOIN dbo.STL0BB b with(nolock) ON (a.CHT_ID=b.CHT_ID AND a.SNDR_ID=b.MMBR_ID)
                WHERE (a.SND_TS<@CurrentDT)
                AND a.CHT_ID=@ChatID AND a.SNDR_ID IN (@SenderID, @ReceiverID) 
                ORDER BY a.SND_TS DESC;
            ", new Dictionary<string, object>(){
                { "ChatKey", DateTime.Now.ToTimeMillisecond().ToString("X") },
                { "SenderID", SenderID },
                { "ReceiverID", ReceiverID },
            });
            if(results != null){
                var chat = results.ReadSingle<dynamic>();
                if(chat!=null){
                    chat.Messages = results.Read<dynamic>();
                    return (Results.Success, chat);
                }
            }
            return (Results.Null, null);
        }


        public async Task<(Results result, object item)> GetPreviousChatAsync(String ChatKey, int StartWith){
            String ReceiverID = $"{ account.PL_ID }{ account.ACT_ID }";
            var results = _repo.DQueryMultiple($@"
                ;with cte as(
                    SELECT TOP(1) a.CHT_ID 
                    FROM dbo.STL0BA a with(nolock) 
                    LEFT JOIN dbo.STL0BB b with(nolock)ON a.CHT_ID=b.CHT_ID
                    WHERE a.CHT_CD=@ChatKey AND b.MMBR_ID=@ReceiverID
                )
                SELECT DISTINCT TOP(15) b.CHT_ID ChatID, b.CHT_CONV_ID ID, b.SNDR_ID SenderID, b.DSPLY_NM DisplayName, b.PROF_IMG_URL ProfileImageUrl
					, b.MSG Message, b.SND_TS DateSend, b.S_FRST_MSG IsFirstMessage
                    , b.S_IMG IsImage, b.S_FIL IsFile, b.MDIA_URL MediaUrl, CAST(IIF(@ReceiverID=b.SNDR_ID,1,0) as bit) IsYou
                FROM cte a with(nolock)
                INNER JOIN dbo.STL0BC b with(nolock)ON a.CHT_ID=b.CHT_ID
                WHERE (b.CHT_CONV_ID<@StartWith OR @StartWith=0)
                ORDER BY SND_TS DESC; 
            ", new Dictionary<string, object>(){
                { "ChatKey", ChatKey },
                { "StartWith", StartWith },
                { "ReceiverID", ReceiverID },
            });
            if(results!=null)
                return (Results.Success, results.Read<dynamic>());
            return (Results.Null, null);
        }


        public async Task<(Results result, object item)> SendMessageAsync(String ChatKey, MessengerAppRequest request){
            String SenderID = $"{ account.PL_ID }{ account.ACT_ID }";
            request.MemberID = $"{account.PL_ID}{request.MemberID}";
            var result = _repo.DQueryMultiple($@"
                DECLARE @IsFirstMessage bit = '0';
                --
                DECLARE @Fullname varchar(150), @DisplayName varchar(150), @ProfileImageUrl varchar(500), @IsPersonal bit=0;
                (SELECT TOP(1) /*@DisplayName=NCK_NM*/ @DisplayName=FLL_NM, @ProfileImageUrl=IMG_URL, @Fullname=FLL_NM
                FROM dbo.STLBDB a with(nolock) 
                WHERE CONCAT(a.PL_ID, a.ACT_ID)=@SenderID) ORDER BY a.PL_ID, a.USR_ID;
                IF(ISNULL(@DisplayName,'')='') SET @DisplayName=@Fullname;
                --
                DECLARE @DateSend datetime=GETDATE();
                DECLARE @ChatID bigint, @IsPublicChat bit;
                --
                SELECT TOP(1) @ChatID=CHT_ID, @IsPublicChat=IIF(ISNULL(S_PBLC,'0')='1' AND ISNULL(S_GRP,'0')='1', 1, 0), @IsPersonal=IIF(ISNULL(S_PRSNL,'0')='1', 1, 0) FROM dbo.STL0BA with(nolock) WHERE CHT_CD=@ChatKey;
                IF(@ChatID IS NOT NULL)
                BEGIN
                    INSERT INTO dbo.STL0BC(CHT_ID, SNDR_ID, DSPLY_NM, PROF_IMG_URL, MSG,  SND_TS, S_FRST_MSG
                        , S_IMG, S_FIL, MDIA_URL)
                    VALUES(@ChatID, @SenderID, @DisplayName, @ProfileImageUrl, @Message, @DateSend, @IsFirstMessage
                        , @IsImage, @IsFile, @MediaUrl);
                    DECLARE @ChatConversationID bigint = SCOPE_IDENTITY();
                    
                    UPDATE TOP(1) dbo.STL0BB SET FLL_NM=@Fullname, DSPLY_NM=@DisplayName, PROF_IMG_URL=@ProfileImageUrl 
                    WHERE CHT_ID=@ChatID AND MMBR_ID=@SenderID;

                    SELECT DISTINCT TOP(15) a.CHT_ID ChatID, a.CHT_CONV_ID ID, a.SNDR_ID SenderID, a.DSPLY_NM DisplayName, a.PROF_IMG_URL ProfileImageUrl
                        , a.MSG Message, a.SND_TS DateSend, a.S_FRST_MSG IsFirstMessage
                        , a.S_IMG IsImage, a.S_FIL IsFile, a.MDIA_URL MediaUrl --, CAST(IIF(@SenderID=a.SNDR_ID,1,0) as bit) IsYou
                        , @IsPublicChat IsPublicChat, @IsPersonal IsPersonal 
                    FROM dbo.STL0BC a with(nolock)
                    WHERE CHT_CONV_ID=@ChatConversationID;
                END 
                ELSE
                BEGIN
                    SELECT '2' RESULT;
                END;
            ", 
            new Dictionary<string, object>(){
                { "ChatKey", ChatKey },
                { "SenderID", SenderID },
                { "Message", request.Message },
                { "IsImage", (request.IsImage?"1":"0") },
                { "IsFile", (request.IsFile?"1":"0") },
                { "MediaUrl", (request.IsImage||request.IsFile?request.MediaUrl:"") },
            }).ReadSingleOrDefault();
            if(result!=null){
                //await notifyChat(ChatKey, result);
                //return (Results.Success, SetAsYou(result));


                var row = ((IDictionary<string, object>)result);
                bool IsPublicChat = row["IsPublicChat"].To<bool>(false);
                bool IsPersonal = row["IsPersonal"].To<bool>(false);
                if (IsPublicChat)
                    await notifyChat(ChatKey, result);
                else if (IsPersonal)
                    await notifyChat(ChatKey, result, request.MemberID);
                return (Results.Success, SetAsYou(result));
            }
            return (Results.Null, null);
        }



        public async Task<(Results result, object items)> GetPreviousChatsAsync(String LastChatTimestamp){
            String ReceiverID = $"{ account.PL_ID }{ account.ACT_ID }";
            var results = _repo.DQueryMultiple($@"
                DECLARE @RowCount int;
                DECLARE @tblConversation table(CHT_CONV_ID bigint, CHT_ID bigint);
                INSERT INTO @tblConversation
                SELECT MAX(a.CHT_CONV_ID)ID, a.CHT_ID
                    FROM dbo.STL0BC a with(nolock)
                    INNER JOIN dbo.STL0BB b with(nolock) ON (a.CHT_ID=b.CHT_ID AND a.SNDR_ID=b.MMBR_ID)
                    INNER JOIN dbo.STL0BB c with(nolock) ON (a.CHT_ID=c.CHT_ID)
                    WHERE a.SND_TS<@LastChatDate AND c.MMBR_ID=@ReceiverID
                    AND a.SNDR_ID IS NOT NULL
                    GROUP BY a.CHT_ID 
                    ORDER BY ID DESC OFFSET 0 ROWS 
                    FETCH NEXT 15 ROWS ONLY;
                SET @RowCount=@@RowCount
                --
                SELECT b.CHT_ID ID, b.CHT_CD ChatKey, b.S_PRSNL IsPersonal, b.S_GRP IsGroup, b.S_PBLC IsPublic, b.S_ALLW_INVT IsAllowInvatiation
                    , c.MMBR_ID MemberID, c.S_ADMN IsAdmin, c.DSPLY_NM DisplayName, c.FLL_NM Fullname, c.PROF_IMG_URL ProfileImageUrl
                FROM @tblConversation a 
                INNER JOIN dbo.STL0BA b with(nolock)ON a.CHT_ID=b.CHT_ID
                INNER JOIN dbo.STL0BB c with(nolock)ON a.CHT_ID=c.CHT_ID 
                WHERE (b.S_PRSNL=1 AND c.MMBR_ID<>@ReceiverID) OR (b.S_GRP=1 AND c.MMBR_ID='0000000000000000');
                --
                SELECT TOP(@RowCount) b.CHT_ID ChatID, b.CHT_CONV_ID ID, b.SNDR_ID SenderID, b.DSPLY_NM DisplayName, b.PROF_IMG_URL ProfileImageUrl
                    , b.MSG Message, b.SND_TS DateSend, b.S_FRST_MSG IsFirstMessage
                    , b.S_IMG IsImage, b.S_FIL IsFile, b.MDIA_URL MediaUrl, CAST(IIF(@ReceiverID=b.SNDR_ID,1,0) as bit) IsYou
                FROM @tblConversation a
                INNER JOIN dbo.STL0BC b with(nolock) ON a.CHT_CONV_ID=b.CHT_CONV_ID AND a.CHT_ID=b.CHT_ID
                ORDER BY SND_TS DESC;
            ", new Dictionary<string, object>(){
                { "LastChatDate", LastChatTimestamp },
                { "ReceiverID", ReceiverID },
            });
            if(results != null){
                var chats = results.Read<dynamic>();
                if(chats.Count() != 0){
                    var messages = results.Read<dynamic>();
                    Dictionary<String, dynamic> msgs = new Dictionary<String, dynamic>();
                    foreach(dynamic message in messages)
                        msgs[(message.ChatID as object).Str()] = message;
                    foreach(dynamic chat in chats)
                        chat.Message = msgs.GetValue((chat.ID as object).Str());
                }
                return (Results.Success, chats);
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, object items)> GetRecentChatsAsync(){
            String ReceiverID = $"{ account.PL_ID }{ account.ACT_ID }";
            var results = _repo.DQueryMultiple($@"
                DECLARE @tblConversation table(
                    rowno int identity, ID bigint, ChatID bigint,
                    SenderID varchar(25), DisplayName varchar(100), ProfileImageUrl varchar(500), 
                    Message nvarchar(500),  DateSend datetime, 
                    IsImage bit, IsFile bit, IsFirstMessage bit
                );
                INSERT INTO @tblConversation(ChatID)
                SELECT DISTINCT b.CHT_ID
                FROM dbo.STL0BB a with(nolock) 
                INNER JOIN dbo.STL0BA b with(nolock) ON a.CHT_ID=b.CHT_ID
                WHERE a.MMBR_ID=@ReceiverID;
                --SELECT * FROM @tblConversation;
                --
                DECLARE @tblChat table(CHT_ID bigint);
                DECLARE @rowcounter int=1, @rowcount int = (@@ROWCOUNT+1);
                WHILE(@rowcounter<@rowcount)
                BEGIN
                    DECLARE @ChatID bigint=null;
                    (SELECT TOP(1)@ChatID=ChatID FROM @tblConversation WHERE rowno=@rowcounter);
                    IF(@ChatID IS NULL)BREAK;
                    DELETE TOP(1) FROM @tblConversation WHERE rowno=@rowcounter;
                    
                    INSERT INTO @tblConversation
                    SELECT a.CHT_CONV_ID, a.CHT_ID, 
                        a.SNDR_ID, a.DSPLY_NM, a.PROF_IMG_URL, 
                        a.MSG, a.SND_TS, 
                        a.S_FRST_MSG, a.S_IMG, a.S_FIL
                    FROM dbo.STL0BC a with(nolock) 
                    INNER JOIN dbo.STL0BB c with(nolock) ON (a.CHT_ID=c.CHT_ID AND a.SNDR_ID=c.MMBR_ID)
                    WHERE (a.CHT_ID=@ChatID AND a.SND_TS>@LatestTimestamp)
                        AND a.SNDR_ID IS NOT NULL
                    ORDER BY a.SND_TS DESC OFFSET 0 ROWS 
                    FETCH NEXT 15 ROWS ONLY;
                    IF(@@ROWCOUNT<>0)
                        INSERT INTO @tblChat
                        VALUES(@ChatID);
                    --
                    SET @rowcounter+=1;
                END;
                --
                --SELECT b.* FROM @tblChat a INNER JOIN Chat b with(nolock)ON a.CHT_ID=b.CHT_ID;
                SELECT b.CHT_ID ID, b.CHT_CD ChatKey, b.S_PRSNL IsPersonal, b.S_GRP IsGroup, b.S_PBLC IsPublic, b.S_ALLW_INVT IsAllowInvatiation
                    , c.MMBR_ID MemberID, c.S_ADMN IsAdmin, c.DSPLY_NM DisplayName, c.FLL_NM Fullname, c.PROF_IMG_URL ProfileImageUrl
                FROM @tblChat a 
                INNER JOIN dbo.STL0BA b with(nolock)ON a.CHT_ID=b.CHT_ID
                INNER JOIN dbo.STL0BB c with(nolock)ON a.CHT_ID=c.CHT_ID 
                WHERE (b.S_PRSNL=1 AND c.MMBR_ID<>@ReceiverID) OR (b.S_GRP=1 AND c.MMBR_ID='0000000000000000');
                --
                SELECT * FROM @tblConversation ORDER BY rowno;
            ", new Dictionary<string, object>(){
                { "LatestTimestamp", DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd HH:mm:ss.fff") },
                { "ReceiverID", ReceiverID },
            });
            if(results != null){
                var chats = results.Read<dynamic>();
                if(chats.Count() != 0){
                    var messages = results.Read<dynamic>();
                    Dictionary<String, dynamic> msgs = new Dictionary<String, dynamic>();
                    foreach(dynamic message in messages){
                        string ChatID = (message.ChatID as object).Str();
                        if(!msgs.ContainsKey(ChatID))
                            msgs[ChatID]= new List<dynamic>();
                        (msgs[ChatID] as List<dynamic>).Add(message);
                    }
                    foreach(dynamic chat in chats)
                        chat.Message = msgs.GetValue((chat.ID as object).Str());
                }
                return (Results.Success, chats);
            }
            return (Results.Null, null);
        }
        //
        private object SetAsYou(IDictionary<string, object> data){
            data["IsYou"] = true;
            return data;
        }
        private async Task<bool> notifyChat(String ChatKey, IDictionary<string, object> row, String MemberID = "")
        {
            //if(row["IsPublicChat"].Str().Equals("1")) return;
            bool IsPublicChat = row["IsPublicChat"].To<bool>(false);
            bool IsPersonal = row["IsPersonal"].To<bool>(false);
            /*var results = _repo.DQueryMultiple($@"
                DECLARE @FRBS_AUTH varchar(250) = (SELECT TOP(1) FRBS_AUTH FROM dbo.STL001 with(nolock) WHERE COMP_ID=@CompanyID)
                SELECT @FRBS_AUTH FRBS_AUTH;
            ", new Dictionary<string, object>(){
                { "CompanyID", account.CompanyID },
            });
            var result = ((IDictionary<string, object>)results.ReadSingleOrDefault());
            String FirebaseAuth = result["FRBS_AUTH"].Str();*/

            if (IsPublicChat){
                var conversation = _repo.DQueryMultiple($@"
                    SELECT TOP(1) a.CHT_ID ID, a.CHT_CD ChatKey, a.S_PRSNL IsPersonal, a.S_GRP IsGroup, a.S_PBLC IsPublic, a.S_ALLW_INVT IsAllowInvatiation
                        , b.MMBR_ID MemberID, S_ADMN IsAdmin, b.DSPLY_NM DisplayName, b.FLL_NM Fullname, b.PROF_IMG_URL ProfileImageUrl
                    FROM dbo.STL0BA a with(nolock)
                    INNER JOIN dbo.STL0BB b with(nolock) ON a.CHT_ID=b.CHT_ID 
                    WHERE a.CHT_CD=@ChatKey
                    AND (a.S_GRP=1 AND b.MMBR_ID='0000000000000000');
                ", new Dictionary<string, object>(){
                    { "ChatKey", ChatKey },
                }).ReadSingleOrDefault();
                conversation.Messages = new List<dynamic>(new[]{row});
                await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/chat", new{ type = "public", content = conversation });
            }
            else if (IsPersonal)
            {
                //string strReceiver=
                var conversation = _repo.DQueryMultiple($@"
                    SELECT TOP(1) a.CHT_ID ID, a.CHT_CD ChatKey, a.S_PRSNL IsPersonal, a.S_GRP IsGroup, a.S_PBLC IsPublic, a.S_ALLW_INVT IsAllowInvatiation
                        , b.MMBR_ID MemberID, S_ADMN IsAdmin, b.DSPLY_NM DisplayName, b.FLL_NM Fullname, b.PROF_IMG_URL ProfileImageUrl
                    FROM dbo.STL0BA a with(nolock)
                    INNER JOIN dbo.STL0BB b with(nolock) ON a.CHT_ID=b.CHT_ID 
                    WHERE a.CHT_CD=@ChatKey
                    AND (a.S_PRSNL=1 AND b.MMBR_ID=@MemberID);
                ", new Dictionary<string, object>(){
                    { "ChatKey", ChatKey },
                    { "MemberID", MemberID },
                }).ReadSingleOrDefault();
                conversation.Messages = new List<dynamic>(new[] { row });
                await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/chat", new { type = "personal", content = conversation });
            }
            else
            {
            }
            return false;
        }

        public async Task<(Results result, object sender)> Load_ChatSender(FilterRequest req)
        {
            var result = _repo.DSpQueryMultiple($"dbo.spfn_0BA0BBBDB01", new Dictionary<string, object>()
            {
                {"parmplid",account.PL_ID },
                {"parmpgrpid",account.PGRP_ID},
                {"parmoptrid",account.USR_ID},
                {"parmrownum",req.num_row},
                {"parmsearch", req.Search }
            });
            if (result != null)
                return (Results.Success, STLSubscriberDto.GetAllChatSenderList(result.Read<dynamic>()));
            return (Results.Null, null);
        }
    }
}